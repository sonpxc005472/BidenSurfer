namespace BidenSurfer.WebApi.Services;

using MassTransit;
using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;

public interface IConfigService
{
    Task<IEnumerable<ConfigDto>> GetConfigsByUser(long userId);
    Task<IEnumerable<ConfigDto>> GetByActiveUser();
    Task<ConfigDto?> GetById(long id);
    Task<bool> AddOrEdit(ConfigDto config);
    Task<bool> SaveNewScanToDb();
    Task<bool> Delete(long id);
    Task<bool> OffConfigs(List<string> customIds);
}

public class ConfigService : IConfigService
{
    private readonly AppDbContext _context;
    private readonly IBus _bus;
    private readonly IRedisCacheService _redisCacheService;

    public ConfigService(AppDbContext context, IBus bus, IRedisCacheService redisCacheService)
    {
        _context = context;
        _bus = bus;
        _redisCacheService = redisCacheService;
    }

    public async Task<bool> AddOrEdit(ConfigDto config)
    {
        var configEntity = await _context.Configs?.FirstOrDefaultAsync(c => c.Id == config.Id);
        var allConfigs = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs);
        if (configEntity == null)
        {
            //Add new
            var configAdd = new Config
            {
                Userid = config.UserId,
                CustomId = config.CustomId,
                Symbol = config.Symbol,
                PositionSide = config.PositionSide,
                OrderChange = config.OrderChange,
                Amount = config.Amount,
                IsActive = config.IsActive,
                AmountLimit = config.AmountLimit,
                IncreaseAmountExpire = config.IncreaseAmountExpire,
                IncreaseAmountPercent = config.IncreaseAmountPercent,
                IncreaseOcPercent = config.IncreaseOcPercent,
                OrderType = config.OrderType,
                CreatedBy = config.CreatedBy,
                Expire = config.Expire
            };
            _context.Configs.Add(configAdd);
            await _context.SaveChangesAsync();

            allConfigs = (await GetByActiveUser()).ToList();
        }
        else
        {
            //Edit
            configEntity.Userid = config.UserId;
            configEntity.Symbol = config.Symbol;
            configEntity.PositionSide = config.PositionSide;
            configEntity.OrderChange = config.OrderChange;
            configEntity.IsActive = config.IsActive;
            configEntity.Amount = config.Amount;
            configEntity.AmountLimit = config.AmountLimit;
            configEntity.IncreaseAmountExpire = config.IncreaseAmountExpire;
            configEntity.IncreaseAmountPercent = config.IncreaseAmountPercent;
            configEntity.OrderType = config.OrderType;
            configEntity.IncreaseOcPercent = config.IncreaseOcPercent;
            configEntity.Expire = config.Expire;
            _context.Configs.Update(configEntity);
            await _context.SaveChangesAsync();

            var configDto = allConfigs.FirstOrDefault(x=>x.Id == config.Id);
            if(configDto != null)
            {
                configDto.Amount = config.Amount;
                configDto.AmountLimit = config.AmountLimit;
                configDto.IncreaseAmountExpire = config.IncreaseAmountExpire;
                configDto.IncreaseAmountPercent= config.IncreaseAmountPercent;
                configDto.IncreaseOcPercent= config.IncreaseOcPercent;
                configDto.IsActive = config.IsActive;
                configDto.OrderChange = config.OrderChange;
                configDto.OrderType = config.OrderType;
                configDto.PositionSide = config.PositionSide;
                configDto.Symbol = config.Symbol;
                configDto.Expire = config.Expire;
            }
        }
        _redisCacheService.SetCachedData(AppConstants.RedisAllConfigs, allConfigs, TimeSpan.FromDays(10));
        await _bus.Send(new RestartBotMessage { CorrelationId = Guid.NewGuid() });
        return true;
    }

    public async Task<bool> Delete(long id)
    {
        var configEntity = await _context.Configs?.FirstOrDefaultAsync(c => c.Id == id);
        if (configEntity == null)
        {
            return false;
        }
        _context.Configs.Remove(configEntity);
        await _context.SaveChangesAsync();
        var allConfigs = await GetByActiveUser();
        var allCachedConfigs = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs);
        var cachedConfig = allCachedConfigs?.FirstOrDefault(c => c.Id == id);
        if(cachedConfig != null)
        {
            allCachedConfigs?.Remove(cachedConfig);
            _redisCacheService.SetCachedData(AppConstants.RedisAllConfigs, allCachedConfigs, TimeSpan.FromDays(10));
        }
        await _bus.Send(new RestartBotMessage { CorrelationId = Guid.NewGuid() });
        return true;
    }

    public async Task<IEnumerable<ConfigDto>> GetConfigsByUser(long userId)
    {
        var result = await _context.Configs?.Include(i => i.User).ThenInclude(c => c.UserSetting).Where(b => b.Userid == userId).ToListAsync() ?? new List<Config>();
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
            IncreaseOcPercent= config.IncreaseOcPercent,
            Expire = config.Expire,
            CreatedBy = config.CreatedBy,
            CreatedDate = config.CreatedDate,
            EditedDate= config.EditedDate
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
            IncreaseAmountExpire=r.IncreaseAmountExpire,        
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

    public async Task<bool> SaveNewScanToDb()
    {
        try
        {
            var allconfigs = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs);
            var newScan = allconfigs?.Where(c => c.isNewScan).ToList();
            if (newScan != null && newScan.Any())
            {
                try
                {
                    var configs = newScan.ConvertAll(c => new Config
                    {
                        Amount = c.Amount,
                        AmountLimit = c.AmountLimit,
                        CreatedBy = c.CreatedBy,
                        CreatedDate = c.CreatedDate,
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
                        Userid = c.UserId
                    });
                    await _context.Configs.AddRangeAsync(configs);
                    await _context.SaveChangesAsync();
                }
                finally
                {
                    foreach (var scan in newScan)
                    {
                        scan.isNewScan = false;
                    }
                    _redisCacheService.SetCachedData(AppConstants.RedisAllConfigs, allconfigs, TimeSpan.FromDays(10));
                }
            }
        }
        catch(Exception ex) {
            return false;
        }        
        
        return true;
    }

    public async Task<bool> OffConfigs(List<string> customIds)
    {
        try
        {
            var configEntity = await _context.Configs?.Where(c => customIds.Contains(c.CustomId)).ToListAsync();
            foreach (var config in configEntity)
            {
                config.IsActive = false;
            }
            _context.UpdateRange(configEntity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}