
using BidenSurfer.Infras;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;

namespace BidenSurfer.BotRunner.Services;
public interface IConfigService
{
    Task<List<ConfigDto>> GetAllActive();
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
            existedConfig.CreatedDate = config.CreatedDate;
            existedConfig.EditedDate = config.EditedDate;
            existedConfig.Expire = config.Expire;
            existedConfig.FilledQuantity = config.FilledQuantity;

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
        _redisCacheService.SetCachedData(AppConstants.RedisAllConfigs, cachedData, TimeSpan.FromDays(100));
    }

    public void DeleteAllConfig()
    {
        _redisCacheService.RemoveCachedData(AppConstants.RedisAllConfigs);
        StaticObject.AllConfigs.Clear();
    }

    public void DeleteConfig(string configId)
    {
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs) ?? new List<ConfigDto>();
        var existedConfig = cachedData.FirstOrDefault(c => c.CustomId == configId);
        if (existedConfig != null)
        {
            cachedData.Remove(existedConfig);
            StaticObject.AllConfigs.RemoveAll(x=>x.CustomId == configId);
        }
        _redisCacheService.SetCachedData(AppConstants.RedisAllConfigs, cachedData, TimeSpan.FromDays(100));
    }

    public async Task<List<ConfigDto>> GetAllActive()
    {
        List<ConfigDto> resultDto = new List<ConfigDto>();
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs);
        if (cachedData != null)
        {
            StaticObject.AllConfigs = cachedData;
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
            StaticObject.AllConfigs = resultDto;
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

    public List<ConfigDto> GetByUserId(long userid)
    {
        return StaticObject.AllConfigs.Where(c => c.UserId == userid).ToList();
    }
}