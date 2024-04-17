
using BidenSurfer.Infras;
using BidenSurfer.Infras.Models;

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
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs) ?? new List<ConfigDto>();
        foreach (var config in configs)
        {
            var existedConfig = cachedData.FirstOrDefault(c => c.CustomId == config.CustomId);
            if (existedConfig == null)
            {
                cachedData.Add(config);
                StaticObject.AllConfigs.Add(config);
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

                var caconfig = StaticObject.AllConfigs.FirstOrDefault(x => x.CustomId == config.CustomId);
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
                }
            }
        }        

        _redisCacheService.SetCachedData(AppConstants.RedisAllConfigs, cachedData, TimeSpan.FromDays(100));
    }    

    public List<ConfigDto> GetAllActive()
    {
        List<ConfigDto> resultDto = new List<ConfigDto>();
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs);
        if (cachedData != null)
        {
            var activeData = cachedData.Where(c => c.IsActive).ToList();
            StaticObject.AllConfigs = activeData;
            return activeData;
        }        
        return resultDto;
    }

    public void OnOffConfig(List<ConfigDto> configs)
    {
        foreach (var config in configs)
        {
            if (!config.IsActive)
            {
                StaticObject.AllConfigs.RemoveAll(c => config.CustomId == c.CustomId && c.CreatedBy == AppConstants.CreatedByScanner);                               
            }
            var cacheData = StaticObject.AllConfigs.FirstOrDefault(c => c.CustomId == config.CustomId);

            if (cacheData != null)
            {
                cacheData.IsActive = config.IsActive;
                cacheData.OrderId = string.Empty;
                cacheData.ClientOrderId = string.Empty;
                cacheData.OrderStatus = null;
                cacheData.isClosingFilledOrder = false;
                cacheData.isNewScan = false;
            }

        }
    }
}