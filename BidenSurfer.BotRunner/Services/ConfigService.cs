
using BidenSurfer.Infras;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Helpers;
using BidenSurfer.Infras.Models;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using CryptoExchange.Net.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace BidenSurfer.BotRunner.Services;
public interface IConfigService
{
    Task<List<ConfigDto>> GetAllActive();
    void AddOrEditConfig(ConfigDto config);
    void AddOrEditConfigFromApi(List<ConfigDto> configs);
    void UpsertConfigs(List<ConfigDto> configs);
    void DeleteAllConfig();
    ConfigWinLose UpsertWinLose(ConfigDto configDto, bool isWin);
    ConfigWinLose GetWinLose(ConfigDto configDto);
    void OnOffConfig(List<ConfigDto> configs);
    void DeleteConfigs(List<string> customIds);
}

public class ConfigService : IConfigService
{
    private readonly IRedisCacheService _redisCacheService;
    private readonly AppDbContext _dbContext;
    private readonly ITeleMessage _teleMessage;

    public ConfigService(IRedisCacheService redisCacheService, AppDbContext dbContext, ITeleMessage teleMessage)
    {
        _redisCacheService = redisCacheService;
        _dbContext = dbContext;
        _teleMessage = teleMessage;
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
        
        ConfigWinLose? winLose;
        if (winDict != null && winDict.ContainsKey(key))
        {
            var win = winDict[key];
            win.Total++;
            if (isWin) win.Win++;
            winDict[key] = win;
            winLose = win;
        }
        else
        {
            winDict ??= new Dictionary<string, ConfigWinLose>();
            winLose = new ConfigWinLose
            {
                Total = 1,
                Win = isWin ? 1 : 0
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

    public async void AddOrEditConfigFromApi(List<ConfigDto> configs)
    {
        try
        {
            foreach (var config in configs)
            {
                var existedConfig = StaticObject.AllConfigs.ContainsKey(config.CustomId);
                if (!existedConfig)
                {
                    StaticObject.AllConfigs.TryAdd(config.CustomId, config);
                }
                else
                {
                    var caconfig = StaticObject.AllConfigs[config.CustomId];
                    if (caconfig != null)
                    {
                        var isOffConfig = !config.IsActive && caconfig.IsActive;
                        caconfig.Amount = config.Amount;
                        caconfig.IncreaseAmountPercent = config.IncreaseAmountPercent;
                        caconfig.IsActive = config.IsActive;
                        caconfig.OrderChange = config.OrderChange;
                        caconfig.IncreaseAmountExpire = config.IncreaseAmountExpire;
                        caconfig.IncreaseOcPercent = config.IncreaseOcPercent;
                        caconfig.AmountLimit = config.AmountLimit;
                        caconfig.EditedDate = config.EditedDate;
                        caconfig.Expire = config.Expire;                        
                        caconfig.OriginAmount = config.OriginAmount;
                        StaticObject.AllConfigs[config.CustomId] = caconfig;
                        if(isOffConfig)
                        {
                            await CancelOrder(caconfig);
                            if(config.CreatedBy == AppConstants.CreatedByScanner)
                            {
                                StaticObject.AllConfigs.TryRemove(config.CustomId, out _);
                            }                            
                        }
                    }
                }
            }            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AddOrEditConfig Error: {ex.Message}");
        }
    }

    public void DeleteConfigs(List<string> customIds)
    {
        foreach (var customId in customIds)
        {
            StaticObject.AllConfigs.TryRemove(customId, out _);
        }
    }

    private async Task<bool> CancelOrder(ConfigDto config)
    {
        try
        {
            var userSetting = StaticObject.AllUsers.FirstOrDefault(u => u.Id == config.UserId)?.Setting;
            BybitRestClient api;
            if (!StaticObject.RestApis.TryGetValue(config.UserId, out api))
            {
                api = new BybitRestClient();

                if (userSetting != null)
                {
                    api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey, ApiCredentialsType.Hmac));
                    StaticObject.RestApis.TryAdd(config.UserId, api);
                }
            }

            if (api != null)
            {
                StaticObject.IsInternalCancel = true;
                var cancelOrder = await api.V5Api.Trading.CancelOrderAsync
                    (
                        Category.Spot,
                        config.Symbol,
                        clientOrderId: config.ClientOrderId
                    );
                config.OrderStatus = null;
                config.ClientOrderId = string.Empty;
                config.OrderId = string.Empty;
                config.isClosingFilledOrder = false;
                config.IsActive = false;
                config.EditedDate = DateTime.Now;
                config.Amount = config.OriginAmount.HasValue ? config.OriginAmount.Value : config.Amount;
                StaticObject.AllConfigs[config.CustomId] = config;

                if (cancelOrder.Success)
                {
                    var message = $"{config.Symbol} | {config.PositionSide.ToUpper()}| {config.OrderChange.ToString()} Cancelled";
                    Console.WriteLine(message);
                    _ = _teleMessage.OffConfigMessage(config.Symbol, config.OrderChange.ToString(), config.PositionSide, userSetting.TeleChannel, "Cancelled");

                    return true;
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now} - Cancel order {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} error: {cancelOrder.Error.Message}");
                }
                await Task.Delay(200);
                StaticObject.IsInternalCancel = false;
            }
        }
        catch (Exception ex)
        {
            // log error to the telegram channels
            Console.WriteLine($"{DateTime.Now} - Cancel order {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} Ex: {ex.Message}");
            StaticObject.IsInternalCancel = false;
            return false;
        }
        return false;
    }
}