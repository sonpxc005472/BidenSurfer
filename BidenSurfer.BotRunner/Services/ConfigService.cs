
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
    void AddOrEditConfig(ConfigDto config);
    void UpsertConfigs(List<ConfigDto> configs);
    void DeleteAllConfig();
    ConfigWinLose UpsertWinLose(ConfigDto configDto, bool isWin);
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
            var existedConfig = StaticObject.AllConfigs.ContainsKey(config.CustomId);
            if (!existedConfig)
            {
                StaticObject.AllConfigs.TryAdd(config.CustomId, config);
            }
            else
            {
                if(!config.IsActive && config.CreatedBy == AppConstants.CreatedByScanner)
                {
                    StaticObject.AllConfigs.TryRemove(config.CustomId, out _);
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
                }                
            }           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AddOrEditConfig Error: {ex.Message}");
        }        
    }

    public void DeleteAllConfig()
    {
        StaticObject.AllConfigs.Clear();
    }

    public void UpsertConfigs(List<ConfigDto> configs)
    {
        try
        {
            foreach (var config in configs)
            {
                if (config.IsActive)
                {
                    AddOrEditConfig(config);
                }
                else
                {
                    if (config.CreatedBy == AppConstants.CreatedByScanner)
                    {
                        StaticObject.AllConfigs.TryRemove(config.CustomId, out _);
                    }                    
                    else
                    {
                        var canGet = StaticObject.AllConfigs.TryGetValue(config.CustomId, out var configsToUpdateMem);
                        if (canGet)
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
        }
        catch (Exception ex)
        {
            Console.WriteLine("Bot Runner UpdateConfigs Error: " + ex.Message);
        }
    }

    public async Task<List<ConfigDto>> GetAllActive()
    {
        try
        {
            var result = await _dbContext.Configs?.Where(b => b.IsActive).ToListAsync() ?? new List<Config>();
            var resultDto = result.Select(r => new ConfigDto
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
            Console.WriteLine("GetAllConfigActive - db: " + resultDto.Count);
            StaticObject.AllConfigs = new ConcurrentDictionary<string, ConfigDto>(resultDto.ToDictionary(c => c.CustomId, c => c));
            return resultDto;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Get all configs Error: " + ex.Message);
            return new List<ConfigDto>();
        }        
    }

    public ConfigWinLose UpsertWinLose(ConfigDto configDto, bool isWin)
    {
        var winDict = _redisCacheService.GetCachedData<Dictionary<string, ConfigWinLose>>(AppConstants.RedisConfigWinLose);
        var key = $"{configDto.UserId}_{configDto.Symbol}_{configDto.PositionSide}_{configDto.OrderChange}";
        var winLose = new ConfigWinLose();
        if (winDict != null && winDict.ContainsKey(key))
        {
            var win = winDict[key];
            win.Total = win.Total + 1;
            if(isWin) win.Win = win.Win + 1;
            winDict[key] = win;
            winLose = win;
        }
        else
        {
            winDict ??= new Dictionary<string, ConfigWinLose>();
            winLose = new ConfigWinLose
            {
                Total = 1,
                Win = isWin? 1 : 0
            };
            winDict.Add(key, winLose);
        }
        _redisCacheService.SetCachedData(AppConstants.RedisConfigWinLose, winDict, TimeSpan.FromDays(100));
        return winLose;
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