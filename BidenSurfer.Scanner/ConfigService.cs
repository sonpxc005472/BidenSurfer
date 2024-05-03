
using BidenSurfer.Infras;
using BidenSurfer.Infras.Models;
using System.Collections.Concurrent;

namespace BidenSurfer.Scanner;
public interface IConfigService
{
    List<ConfigDto> GetAllActive();
    void AddOrEditConfig(List<ConfigDto> configs);
    void OnOffConfig(List<ConfigDto> configs);
}

public class ConfigService : IConfigService
{
    private readonly IRedisCacheService _redisCacheService;

    public ConfigService(IRedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    public void AddOrEditConfig(List<ConfigDto> configs)
    {
        try
        {
            var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs) ?? new List<ConfigDto>();
            foreach (var config in configs)
            {
                var existedConfig = cachedData.FirstOrDefault(c => c.CustomId == config.CustomId);
                if (existedConfig == null)
                {
                    cachedData.Add(config);
                    StaticObject.AllConfigs.TryAdd(config.CustomId, config);
                }
                else
                {
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
                    existedConfig.Expire = config.Expire;
                    existedConfig.CreatedBy = config.CreatedBy;
                    existedConfig.CreatedDate = config.CreatedDate;
                    existedConfig.EditedDate = config.EditedDate;
                    if (StaticObject.AllConfigs.ContainsKey(config.CustomId))
                    {
                        var caconfig = StaticObject.AllConfigs[config.CustomId];
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
                        caconfig.TotalQuantity = config.TotalQuantity;
                        caconfig.isNewScan = config.isNewScan;
                        caconfig.isClosingFilledOrder = config.isClosingFilledOrder;
                        StaticObject.AllConfigs[config.CustomId] = caconfig;
                    }
                }
            }

            _redisCacheService.SetCachedData(AppConstants.RedisAllConfigs, cachedData, TimeSpan.FromDays(100));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Scanner Add/Edit config Error: " + ex.Message);
        }
    }    

    public List<ConfigDto> GetAllActive()
    {
        try
        {
            List<ConfigDto> resultDto = new List<ConfigDto>();
            var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs);
            if (cachedData != null)
            {
                var activeData = cachedData.Where(c => c.IsActive).ToList();
                StaticObject.AllConfigs = new ConcurrentDictionary<string, ConfigDto>(activeData.ToDictionary(c => c.CustomId, c => c));
                return activeData;
            }
            return resultDto;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }        
    }

    public void OnOffConfig(List<ConfigDto> configs)
    {
        try
        {
            foreach (var config in configs)
            {
                if (!config.IsActive && config.CreatedBy == AppConstants.CreatedByScanner)
                {
                    StaticObject.AllConfigs.TryRemove(config.CustomId, out _);
                }
                else if(StaticObject.AllConfigs.ContainsKey(config.CustomId))
                {
                    var cacheData = StaticObject.AllConfigs[config.CustomId];
                    cacheData.IsActive = config.IsActive;
                    if(!config.IsActive)
                    {
                        cacheData.OrderId = string.Empty;
                        cacheData.ClientOrderId = string.Empty;
                        cacheData.OrderStatus = null;
                        cacheData.isClosingFilledOrder = false;
                        cacheData.isNewScan = false;
                        cacheData.Amount = cacheData.OriginAmount ?? cacheData.Amount;
                        cacheData.EditedDate = DateTime.Now;
                    }
                    
                    StaticObject.AllConfigs[config.CustomId] = cacheData;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Scanner On/Off config Ex: " + ex.Message);
        }        
    }
}