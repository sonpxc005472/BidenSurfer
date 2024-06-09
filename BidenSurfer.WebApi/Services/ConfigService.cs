namespace BidenSurfer.WebApi.Services;

using MassTransit;
using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using Bybit.Net.Objects.Models.V5;

public interface IConfigService
{
    Task<IEnumerable<ConfigDto>> GetConfigsByUser(long userId);
    Task<IEnumerable<ConfigDto>> GetByActiveUser();
    Task<ConfigDto?> GetById(long id);
    Task<bool> AddOrEdit(AddEditConfigDto config, bool fromBotUpdate);
    Task<bool> SaveNewScanToDb(List<ConfigDto> configs);
    Task<bool> Delete(long id);
    Task<bool> SetConfigActiveStatus(long id, bool isActive);
    Task<bool> OffConfigs(List<string> customIds);
    Task<bool> OffAllConfigs();
    Task<List<SymbolDto>> GetAllMarginSymbol();
    Task AmountExpireUpdate(List<string> customIds);
}

public class ConfigService : IConfigService
{
    private readonly AppDbContext _context;
    private readonly IBus _bus;
    private ISecurityContextAccessor _securityContextAccessor;
    private readonly ILogger<ConfigService> _logger;

    public ConfigService(AppDbContext context, IBus bus, ISecurityContextAccessor securityContextAccessor, ILogger<ConfigService> logger)
    {
        _context = context;
        _bus = bus;
        _securityContextAccessor = securityContextAccessor;
        _logger = logger;
    }

    public async Task<bool> AddOrEdit(AddEditConfigDto config, bool fromBotUpdate)
    {
        var configDto = new ConfigDto();
        var configEntity = await _context.Configs?.FirstOrDefaultAsync(c => c.Id == config.Id || c.CustomId == config.CustomId);
        if (configEntity == null)
        {
            //Add new
            var userId = _securityContextAccessor.UserId;
            var configAdd = new Config
            {
                Userid = userId,
                CustomId = Guid.NewGuid().ToString(),
                Symbol = config.Symbol,
                PositionSide = config.PositionSide,
                OrderChange = config.OrderChange,
                Amount = config.Amount,
                IsActive = config.IsActive,
                AmountLimit = config.AmountLimit,
                IncreaseAmountExpire = config.IncreaseAmountExpire,
                IncreaseAmountPercent = config.IncreaseAmountPercent,
                IncreaseOcPercent = config.IncreaseOcPercent,
                OrderType = (int)OrderTypeEnums.Margin,
                CreatedBy = AppConstants.CreatedByUser,
                Expire = config.Expire,
                OriginAmount = config.Amount,
                CreatedDate = DateTime.Now,
                EditedDate = DateTime.Now
            };
            _context.Configs.Add(configAdd);
            await _context.SaveChangesAsync();
            configDto = configAdd.ToDto();
        }
        else
        {
            configEntity.Symbol = config.Symbol;
            configEntity.PositionSide = config.PositionSide;
            configEntity.OrderChange = config.OrderChange;
            configEntity.IsActive = config.IsActive;
            configEntity.Amount = config.Amount;
            configEntity.AmountLimit = config.AmountLimit;
            configEntity.IncreaseAmountExpire = config.IncreaseAmountExpire;
            configEntity.IncreaseAmountPercent = config.IncreaseAmountPercent;
            configEntity.IncreaseOcPercent = config.IncreaseOcPercent;
            configEntity.Expire = config.Expire;
            configEntity.EditedDate = DateTime.Now;
            configEntity.OriginAmount = fromBotUpdate ? configEntity.OriginAmount : config.Amount;
            if (!config.IsActive && configEntity.CreatedBy == AppConstants.CreatedByScanner)
            {
                _context.Configs.Remove(configEntity);
            }
            else
            {
                //Edit
                _context.Configs.Update(configEntity);
            }
            
            await _context.SaveChangesAsync();
            configDto = configEntity.ToDto();
        }

        if(!fromBotUpdate)
        {
            _ = _bus.Send(new ConfigUpdateFromApiForBotRunnerMessage
            {
                ConfigDtos = new List<ConfigDto>
            {
                configDto
            }
            });
            _ = _bus.Send(new ConfigUpdateFromApiForScannerMessage
            {
                ConfigDtos = new List<ConfigDto>
            {
                configDto
            }
            });
        }    
        
        return true;
    }

    public async Task<bool> Delete(long id)
    {
        var configEntity = await _context.Configs?.FirstOrDefaultAsync(c => c.Id == id);
        if (configEntity == null || (configEntity != null && configEntity.IsActive))
        {
            return false;
        }
        _context.Configs.Remove(configEntity);
        await _context.SaveChangesAsync();
        _ = _bus.Send(new ConfigUpdateFromApiForBotRunnerMessage
        {
            ConfigDtos = new List<ConfigDto>
            {
                configEntity.ToDto()
            },
            IsDelete = true
        });
        _ = _bus.Send(new ConfigUpdateFromApiForScannerMessage
        {
            ConfigDtos = new List<ConfigDto>
            {
                configEntity.ToDto()
            },
            IsDelete = true
        });
        return true;
    }

    public async Task<IEnumerable<ConfigDto>> GetConfigsByUser(long userId)
    {
        var result = await _context.Configs.Where(b => b.Userid == userId).OrderBy(x => x.Symbol).ThenBy(x => x.PositionSide).ThenBy(x => x.OrderChange).ToListAsync() ?? new List<Config>();
        return result.Select(r => new ConfigDto
        {
            Id = r.Id,
            UserId = r.Userid,
            PositionSide = r.PositionSide,
            Symbol = r.Symbol,
            OrderChange = r.OrderChange,
            IsActive = r.IsActive,
            Amount = r.Amount,
            IncreaseAmountExpire = r.IncreaseAmountExpire,
            IncreaseOcPercent = r.IncreaseOcPercent,
            OrderType = r.OrderType,
            IncreaseAmountPercent = r.IncreaseAmountPercent,
            AmountLimit = r.AmountLimit,
            Expire = r.Expire,
            CreatedBy = r.CreatedBy,
            CreatedDate = r.CreatedDate,
            EditedDate = r.EditedDate
        });
    }

    public async Task<ConfigDto?> GetById(long id)
    {
        var config = await _context.Configs?.FirstOrDefaultAsync(x => x.Id == id);
        if (null == config) return null;
        return new ConfigDto
        {
            Id = config.Id,
            UserId = config.Userid,
            PositionSide = config.PositionSide,
            Symbol = config.Symbol,
            OrderChange = config.OrderChange,
            IsActive = config.IsActive,
            Amount = config.Amount,
            AmountLimit = config.AmountLimit,
            IncreaseAmountPercent = config.IncreaseAmountPercent,
            OrderType = config.OrderType,
            IncreaseAmountExpire = config.IncreaseAmountExpire,
            IncreaseOcPercent = config.IncreaseOcPercent,
            Expire = config.Expire,
            CreatedBy = config.CreatedBy,
            CreatedDate = config.CreatedDate,
            EditedDate = config.EditedDate
        };
    }

    public async Task<IEnumerable<ConfigDto>> GetByActiveUser()
    {
        List<ConfigDto> resultDto;
        var result = await _context.Configs?.Include(i => i.User).ThenInclude(c => c.UserSetting).Where(b => b.User.Status == (int)UserStatusEnums.Active).ToListAsync() ?? new List<Config>();
        resultDto = result.Select(r => new ConfigDto
        {
            Id = r.Id,
            CustomId = r.CustomId,
            UserId = r.Userid,
            PositionSide = r.PositionSide,
            Symbol = r.Symbol,
            OrderChange = r.OrderChange,
            IsActive = r.IsActive,
            Amount = r.Amount,
            OrderType = r.OrderType,
            AmountLimit = r.AmountLimit,
            IncreaseAmountPercent = r.IncreaseAmountPercent,
            IncreaseOcPercent = r.IncreaseOcPercent,
            IncreaseAmountExpire = r.IncreaseAmountExpire,
            CreatedBy = r.CreatedBy,
            CreatedDate = r.CreatedDate,
            EditedDate = r.EditedDate,
            Expire = r.Expire,
            UserDto = new UserDto
            {
                Id = r.Userid,
                FullName = r.User?.FullName,
                Username = r.User?.Username,
                Email = r.User?.Email,
                Status = r.User?.Status ?? 0,
                Role = r.User?.Role ?? 0,
                Setting = new UserSettingDto
                {
                    Id = r.User.UserSetting.Id,
                    ApiKey = r.User.UserSetting.ApiKey,
                    SecretKey = r.User.UserSetting.SecretKey,
                    PassPhrase = r.User.UserSetting.PassPhrase,
                    TeleChannel = r.User.UserSetting.TeleChannel
                }
            }
        }).ToList();
        return resultDto;
    }

    public async Task<bool> SaveNewScanToDb(List<ConfigDto> newScans)
    {
        try
        {
            if (newScans != null && newScans.Any())
            {
                var configs = newScans.ConvertAll(c => new Config
                {
                    Amount = c.Amount,
                    AmountLimit = c.AmountLimit,
                    CreatedBy = c.CreatedBy,
                    CreatedDate = c.CreatedDate,
                    EditedDate = c.EditedDate,
                    CustomId = c.CustomId,
                    Expire = c.Expire,
                    IncreaseAmountExpire = c.IncreaseAmountExpire,
                    IncreaseAmountPercent = c.IncreaseAmountPercent,
                    IncreaseOcPercent = c.IncreaseOcPercent,
                    IsActive = true,
                    OrderChange = c.OrderChange,
                    OrderType = c.OrderType,
                    PositionSide = c.PositionSide,
                    Symbol = c.Symbol,
                    Userid = c.UserId,
                    OriginAmount = c.Amount
                });
                await _context.Configs.AddRangeAsync(configs);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> OffConfigs(List<string> customIds)
    {
        try
        {
            _logger.LogInformation("OffConfigs: " + string.Join(",", customIds));
            var configEntities = await _context.Configs?.Where(c => customIds.Contains(c.CustomId)).ToListAsync();
            var toRemove = configEntities?.Where(c => c.CreatedBy == AppConstants.CreatedByScanner).ToList();
            var toUpdate = configEntities?.Where(c => c.CreatedBy != AppConstants.CreatedByScanner).ToList();
            if (toRemove.Any())
            {
                _logger.LogInformation("Delete configs: " + string.Join(",", toRemove.Select(c => $"{c.Symbol} - {c.CustomId}").ToList()));
                _context.Configs.RemoveRange(toRemove);
            }
            if (toUpdate.Any())
            {
                _logger.LogInformation("Update configs: " + string.Join(",", toUpdate.Select(c => $"{c.Symbol} - {c.CustomId}").ToList()));
                foreach (var entity in toUpdate)
                {
                    entity.IsActive = false;
                    entity.EditedDate = DateTime.Now;
                    entity.Amount = entity.OriginAmount ?? entity.Amount;
                }
                _context.Configs.UpdateRange(toUpdate);
            }
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("OffConfigs Error: " + ex.Message);
            return false;
        }
    }

    public async Task AmountExpireUpdate(List<string> customIds)
    {
        try
        {
            _logger.LogInformation("AmountExpireUpdate: " + string.Join(",", customIds));
            var configEntities = await _context.Configs?.Where(c => customIds.Contains(c.CustomId)).ToListAsync();
            if (!configEntities.Any())
            {
                _logger.LogInformation("AmountExpireUpdate: No config found");
                return;
            }
            foreach (var entity in configEntities)
            {
                entity.Amount = entity.OriginAmount.HasValue ? entity.OriginAmount.Value : entity.Amount;
                entity.EditedDate = DateTime.Now;
                _logger.LogInformation($"AmountExpireUpdate: {entity.Symbol} - {entity.Amount}");
            }
            _context.Configs.UpdateRange(configEntities);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("AmountExpireUpdate Error: " + ex.Message);
        }
    }

    public async Task<List<SymbolDto>> GetAllMarginSymbol()
    {
        try
        {
            var publicApi = new BybitRestClient();
            var spotSymbols = (await publicApi.V5Api.ExchangeData.GetSpotSymbolsAsync()).Data.List;
            var symbolInfos = spotSymbols.Where(c => (c.MarginTrading == MarginTrading.Both || c.MarginTrading == MarginTrading.UtaOnly) && c.Name.EndsWith("USDT")).ToList() ?? new List<BybitSpotSymbol>();
            var marginSymbols = symbolInfos.Select(c => new { c.Name, c.BaseAsset }).Distinct().OrderBy(c => c.BaseAsset).ToList();
            _ = _bus.Send(new SymbolInfoUpdateForBotRunnerMessage { Symbols = symbolInfos });
            _ = _bus.Send(new SymbolInfoUpdateForScannerMessage { Symbols = symbolInfos });

            return marginSymbols.ConvertAll(s => new SymbolDto { Value = s.Name, Label = s.BaseAsset });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return new List<SymbolDto>();
        }
    }

    public async Task<bool> SetConfigActiveStatus(long id, bool isActive)
    {
        try
        {
            var config = await _context.Configs?.FirstOrDefaultAsync(x => x.Id == id);
            if (null == config) return false;
            config.IsActive = isActive;
            if(config.CreatedBy == AppConstants.CreatedByScanner && !isActive)
            {
                _context.Configs.Remove(config);
            }
            else
            {
                _context.Configs.Update(config);
            }
            
            await _context.SaveChangesAsync();
            _ = _bus.Send(new ConfigUpdateFromApiForBotRunnerMessage { ConfigDtos = new List<ConfigDto> { config.ToDto() } });
            _ = _bus.Send(new ConfigUpdateFromApiForScannerMessage { ConfigDtos = new List<ConfigDto> { config.ToDto() } });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> OffAllConfigs()
    {
        try
        {
            var configs = await _context.Configs?.Where(x => x.IsActive).ToListAsync();
            if (configs.Any())
            {
                foreach (var config in configs)
                {
                    config.IsActive = false;
                    config.EditedDate = DateTime.Now;
                    config.Amount = config.OriginAmount ?? config.Amount;
                }
                _context.Configs.UpdateRange(configs);
                await _context.SaveChangesAsync();
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("OffAllConfigs Error: " + ex.Message);
            return false;
        }   
    }
}