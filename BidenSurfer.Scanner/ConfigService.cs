
using BidenSurfer.Infras;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BidenSurfer.Scanner;
public interface IConfigService
{
    Task<List<ConfigDto>> GetAllActiveAsync();
    void AddOrEditConfig(List<ConfigDto> configs);
    void Delete(string customId);
    void OnOffConfig(List<ConfigDto> configs);
}

public class ConfigService : IConfigService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ConfigService> _logger;

    public ConfigService(AppDbContext dbContext, ILogger<ConfigService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public void AddOrEditConfig(List<ConfigDto> configs)
    {
        try
        {
            foreach (var config in configs)
            {
                var canGet = StaticObject.AllConfigs.TryGetValue(config.CustomId, out var existedConfig);
                if (!canGet)
                {                   
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
                    if(!config.IsActive && config.CreatedBy == AppConstants.CreatedByScanner)
                    {
                        StaticObject.AllConfigs.TryRemove(config.CustomId, out _);
                    }
                    else
                    {
                        StaticObject.AllConfigs[config.CustomId] = existedConfig;
                    }                    
                }
            }

        }
        catch (Exception ex)
        {
            _logger.LogInformation("Scanner Add/Edit config Error: " + ex.Message);
        }
    }

    public void Delete(string customId)
    {
        StaticObject.AllConfigs.TryRemove(customId, out _);
    }

    public async Task<List<ConfigDto>> GetAllActiveAsync()
    {
        try
        {
            var result = await _dbContext.Configs.AsNoTracking().Where(b => b.IsActive).ToListAsync() ?? new List<Config>();
            var resultDto = result.ConvertAll(r => new ConfigDto
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
            _logger.LogInformation($"Get all active configs: {resultDto.Count}");
            StaticObject.AllConfigs = new ConcurrentDictionary<string, ConfigDto>(resultDto.ToDictionary(c => c.CustomId, c => c));
            return resultDto;
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Get all configs Error: " + ex.Message);
            return new List<ConfigDto>();
        }
    }

    public void OnOffConfig(List<ConfigDto> configs)
    {
        try
        {
            _logger.LogInformation($"OnOffConfig {string.Join(",", configs.Select(c => c.CustomId).ToList())}");
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
            _logger.LogError("Scanner On/Off config Ex: " + ex.Message);
        }        
    }
}