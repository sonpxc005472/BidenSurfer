
using BidenSurfer.Infras;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq;

namespace BidenSurfer.BotRunner.Services;
public interface IConfigService
{
    Task<List<ConfigDto>> GetAllActive();
    ConfigDto GetById(long id);
    void AddOrEditConfig(ConfigDto config);
    void UpdateConfig(List<ConfigDto> configs);
    void DeleteAllConfig();
    void UpsertWinLose(ConfigDto configDto, bool isWin);
    ConfigWinLose GetWinLose(ConfigDto configDto);
    void OnOffConfig(List<ConfigDto> configs);
}

public class ConfigService : IConfigService
{
    private readonly IRedisCacheService _redisCacheService;
    private readonly AppDbContext _dbContext;

    public ConfigService(IRedisCacheService redisCacheService, AppDbContext dbContext)
    {
        _redisCacheService = redisCacheService;
        _dbContext = dbContext;
    }

    public void AddOrEditConfig(ConfigDto config)
    {
        try
        {
            var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs) ?? new List<ConfigDto>();
            var existedConfig = cachedData.FirstOrDefault(c => c.CustomId == config.CustomId);
            if (existedConfig == null)
            {
                cachedData.Add(config);
                StaticObject.AllConfigs.TryAdd(config.CustomId, config);
            }
            else
            {
                if(!config.IsActive && config.CreatedBy == AppConstants.CreatedByScanner)
                {
                    StaticObject.AllConfigs.TryRemove(config.CustomId, out _);
                    cachedData.RemoveAll(c => c.CustomId == config.CustomId);
                }
                else
                {
                    var caconfig = StaticObject.AllConfigs[config.CustomId];
                    if (caconfig != null)
                    {
                        caconfig.Amount = config.Amount;
                        caconfig.IncreaseAmountPercent = config.IncreaseAmountPercent;
                        caconfig.IsActive = config.IsActive;
                        caconfig.OrderChange = config.OrderChange;
                        caconfig.IncreaseAmountExpire = config.IncreaseAmountExpire;
                        caconfig.IncreaseOcPercent = config.IncreaseOcPercent;
                        caconfig.AmountLimit = config.AmountLimit;
                        caconfig.FilledPrice = config.FilledPrice;
                        caconfig.OrderId = config.OrderId;
                        caconfig.ClientOrderId = config.ClientOrderId;
                        caconfig.TPPrice = config.TPPrice;
                        caconfig.OrderStatus = config.OrderStatus;
                        caconfig.CreatedDate = config.CreatedDate;
                        caconfig.EditedDate = config.EditedDate;
                        caconfig.Expire = config.Expire;
                        caconfig.FilledQuantity = config.FilledQuantity;
                        caconfig.isClosingFilledOrder = config.isClosingFilledOrder;
                        caconfig.OriginAmount = config.OriginAmount;
                        StaticObject.AllConfigs[config.CustomId] = caconfig;
                    }
                    existedConfig.Amount = config.Amount;
                    existedConfig.IncreaseAmountPercent = config.IncreaseAmountPercent;
                    existedConfig.IsActive = config.IsActive;
                    existedConfig.OrderChange = config.OrderChange;
                    existedConfig.IncreaseAmountExpire = config.IncreaseAmountExpire;
                    existedConfig.IncreaseOcPercent = config.IncreaseOcPercent;
                    existedConfig.AmountLimit = config.AmountLimit;
                    existedConfig.FilledPrice = config.FilledPrice;
                    existedConfig.OrderId = config.OrderId;
                    existedConfig.ClientOrderId = config.ClientOrderId;
                    existedConfig.TPPrice = config.TPPrice;
                    existedConfig.OrderStatus = config.OrderStatus;
                    existedConfig.CreatedDate = config.CreatedDate;
                    existedConfig.EditedDate = config.EditedDate;
                    existedConfig.Expire = config.Expire;
                    existedConfig.FilledQuantity = config.FilledQuantity;
                    existedConfig.isClosingFilledOrder = config.isClosingFilledOrder;
                    existedConfig.OriginAmount = config.OriginAmount;                    
                }
                
            }
            _redisCacheService.SetCachedData(AppConstants.RedisAllConfigs, cachedData, TimeSpan.FromDays(100));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AddOrEditConfig Error: {ex.Message}");
        }        
    }

    public void DeleteAllConfig()
    {
        _redisCacheService.RemoveCachedData(AppConstants.RedisAllConfigs);
        StaticObject.AllConfigs.Clear();
    }

    public void UpdateConfig(List<ConfigDto> configs)
    {
        try
        {
            var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs) ?? new List<ConfigDto>();

            foreach (var config in configs)
            {
                if (config.IsActive)
                {
                    AddOrEditConfig(config);
                }
                else
                {
                    cachedData.RemoveAll(c => config.CustomId == c.CustomId && c.CreatedBy == AppConstants.CreatedByScanner);
                    if (config.CreatedBy == AppConstants.CreatedByScanner)
                    {
                        StaticObject.AllConfigs.TryRemove(config.CustomId, out _);
                    }

                    var configToUpdate = cachedData.FirstOrDefault(c => config.CustomId == c.CustomId && c.CreatedBy != AppConstants.CreatedByScanner);
                    if (configToUpdate != null)
                    {
                        configToUpdate.IsActive = false;
                        configToUpdate.OrderId = string.Empty;
                        configToUpdate.ClientOrderId = string.Empty;
                        configToUpdate.OrderStatus = null;
                        configToUpdate.EditedDate = DateTime.Now;
                        configToUpdate.isClosingFilledOrder = false;
                        configToUpdate.Amount = configToUpdate.OriginAmount ?? configToUpdate.Amount;
                    };
                    if (config.CreatedBy != AppConstants.CreatedByScanner)
                    {
                        var configsToUpdateMem = StaticObject.AllConfigs[config.CustomId];
                        if (configsToUpdateMem != null)
                        {
                            configsToUpdateMem.IsActive = false;
                            configsToUpdateMem.OrderId = string.Empty;
                            configsToUpdateMem.ClientOrderId = string.Empty;
                            configsToUpdateMem.OrderStatus = null;
                            configsToUpdateMem.EditedDate = DateTime.Now;
                            configsToUpdateMem.isClosingFilledOrder = false;
                            configsToUpdateMem.Amount = configsToUpdateMem.OriginAmount ?? configsToUpdateMem.Amount;
                            StaticObject.AllConfigs[config.CustomId] = configsToUpdateMem;
                        }
                    }

                }
            }
            _redisCacheService.SetCachedData(AppConstants.RedisAllConfigs, cachedData, TimeSpan.FromDays(100));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Bot Runner UpdateConfigs Error: " + ex.Message);
        }
    }

    public async Task<List<ConfigDto>> GetAllActive()
    {
        List<ConfigDto> resultDto = new List<ConfigDto>();
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs);
        if (cachedData != null)
        {
            StaticObject.AllConfigs = new ConcurrentDictionary<string, ConfigDto>(cachedData.ToDictionary(c => c.CustomId, c => c));
            return cachedData;
        }
        else
        {
            var result = await _dbContext.Configs?.Include(i => i.User).ThenInclude(c => c.UserSetting).Where(b => b.User.Status == (int)UserStatusEnums.Active && b.IsActive).ToListAsync() ?? new List<Config>();
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
                OriginAmount = r.OriginAmount,
                OrderType = r.OrderType,
                AmountLimit = r.AmountLimit,
                IncreaseAmountPercent = r.IncreaseAmountPercent,
                IncreaseOcPercent = r.IncreaseOcPercent,
                IncreaseAmountExpire = r.IncreaseAmountExpire,
                CreatedBy = r.CreatedBy,
                CreatedDate = r.CreatedDate,
                EditedDate = r.EditedDate,
                Expire = r.Expire                
            }).ToList();
            StaticObject.AllConfigs = new ConcurrentDictionary<string, ConfigDto>(resultDto.ToDictionary(c => c.CustomId, c => c));
            _redisCacheService.SetCachedData(AppConstants.RedisAllConfigs, resultDto, TimeSpan.FromDays(10));
        }
        return resultDto;
    }

    public ConfigDto GetById(long id)
    {
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs);
        if (cachedData != null)
        {
            return cachedData.FirstOrDefault(c => c.Id == id);
        }
        
        return null;
    }

    public void UpsertWinLose(ConfigDto configDto, bool isWin)
    {
        var winDict = _redisCacheService.GetCachedData<Dictionary<string, ConfigWinLose>>(AppConstants.RedisConfigWinLose);
        var key = $"{configDto.UserId}_{configDto.Symbol}_{configDto.PositionSide}_{configDto.OrderChange}";
        if (winDict != null && winDict.ContainsKey(key))
        {
            var win = winDict[key];
            win.Total = win.Total + 1;
            if(isWin) win.Win = win.Win + 1;
            winDict[key] = win;
        }
        else
        {
            winDict ??= new Dictionary<string, ConfigWinLose>();
            winDict.Add(key, new ConfigWinLose
            {
                Total = 1,
                Win = isWin? 1 : 0
            });
        }
        _redisCacheService.SetCachedData(AppConstants.RedisConfigWinLose, winDict, TimeSpan.FromDays(100));
    }

    public ConfigWinLose GetWinLose(ConfigDto configDto)
    {
        var key = $"{configDto.UserId}_{configDto.Symbol}_{configDto.PositionSide}_{configDto.OrderChange}";
        var winDict = _redisCacheService.GetCachedData<Dictionary<string, ConfigWinLose>>(AppConstants.RedisConfigWinLose);
        if (winDict != null && winDict.ContainsKey(key))
        {
            return winDict[key];
        }
        return new ConfigWinLose();
    }

    public void OnOffConfig(List<ConfigDto> configs)
    {
        try
        {
            foreach (var config in configs)
            {
                if (!config.IsActive)
                {
                    if(config.CreatedBy == AppConstants.CreatedByScanner)
                    {
                        StaticObject.AllConfigs.TryRemove(config.CustomId, out _);
                    }                    
                }
                var cacheData = StaticObject.AllConfigs[config.CustomId];

                if (cacheData != null)
                {
                    cacheData.IsActive = config.IsActive;
                    cacheData.OrderId = string.Empty;
                    cacheData.ClientOrderId = string.Empty;
                    cacheData.OrderStatus = null;
                    cacheData.isClosingFilledOrder = false;
                    cacheData.isNewScan = false;
                    cacheData.EditedDate = DateTime.Now;
                    StaticObject.AllConfigs[config.CustomId] = cacheData;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"On/Off Config Error: {ex.Message}");
        }        
    }
}