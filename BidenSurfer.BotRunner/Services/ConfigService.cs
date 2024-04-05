
using BidenSurfer.Infras;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;

namespace BidenSurfer.BotRunner.Services;
public interface IConfigService
{
    List<ConfigDto> GetAllActive();
    ConfigDto GetById(long id);
    List<ConfigDto> GetByUserId(long userid);
    void AddOrEditConfig(ConfigDto config);
    void DeleteConfig(string configId);
    void DeleteAllConfig();
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
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs) ?? new List<ConfigDto>();
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
            existedConfig.CreatedDate = config.CreatedDate;
            existedConfig.EditedDate = config.EditedDate;
            existedConfig.Expire = config.Expire;
        }

        _redisCacheService.SetCachedData(AppConstants.RedisAllConfigs, cachedData, TimeSpan.FromDays(100));
    }

    public void DeleteAllConfig()
    {
        _redisCacheService.RemoveCachedData(AppConstants.RedisAllConfigs);
    }

    public void DeleteConfig(string configId)
    {
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs) ?? new List<ConfigDto>();
        var existedConfig = cachedData.FirstOrDefault(c => c.CustomId == configId);
        if (existedConfig != null)
        {
            cachedData.Remove(existedConfig);
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

    public ConfigDto GetById(long id)
    {
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs);
        if (cachedData != null)
        {
            return cachedData.FirstOrDefault(c => c.Id == id);
        }
        
        return null;
    }

    public List<ConfigDto> GetByUserId(long userid)
    {
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs);
        if (cachedData != null)
        {
            return cachedData.Where(c => c.UserId == userid).ToList();
        }
        return new List<ConfigDto>();
    }
}