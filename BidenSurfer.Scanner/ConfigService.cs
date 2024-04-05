
using BidenSurfer.Infras;
using BidenSurfer.Infras.Models;

namespace BidenSurfer.Scanner;
public interface IConfigService
{
    List<ConfigDto> GetAllActive();
    void AddOrEditConfig(List<ConfigDto> configs);
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
            return cachedData.Where(c=>c.IsActive).ToList();
        }        
        return resultDto;
    }    
}