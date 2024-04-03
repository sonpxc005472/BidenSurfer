
using BidenSurfer.Infras;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;

namespace BidenSurfer.Bot.Services;
public interface IConfigService
{
    Task<List<ConfigDto>> GetAllActive();
    ConfigDto GetById(long id);
    List<ConfigDto> GetByUserId(long userid);
    void AddOrEditConfig(ConfigDto config);
    void DeleteConfig(long configId);
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
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(Constants.RedisAllConfigs) ?? new List<ConfigDto>();
        var existedConfig = cachedData.FirstOrDefault(c => c.Id == config.Id);
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
        }

        _redisCacheService.SetCachedData(Constants.RedisAllConfigs, cachedData, TimeSpan.FromDays(100));
    }

    public void DeleteAllConfig()
    {
        _redisCacheService.RemoveCachedData(Constants.RedisAllConfigs);
    }

    public void DeleteConfig(long configId)
    {
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(Constants.RedisAllConfigs) ?? new List<ConfigDto>();
        var existedConfig = cachedData.FirstOrDefault(c => c.Id == configId);
        if (existedConfig != null)
        {
            cachedData.Remove(existedConfig);
        }
        _redisCacheService.SetCachedData(Constants.RedisAllConfigs, cachedData, TimeSpan.FromDays(100));
    }

    public async Task<List<ConfigDto>> GetAllActive()
    {
        List<ConfigDto> resultDto = new List<ConfigDto>();
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(Constants.RedisAllConfigs);
        if (cachedData != null)
        {
            return cachedData.Where(c=>c.IsActive).ToList();
        }
        var result = await _dbContext.Configs?.Include(i => i.User).ThenInclude(c => c.UserSetting).Where(b => b.IsActive).ToListAsync() ?? new List<Config>();
        resultDto = result.Select(r => new ConfigDto
        {
            Id = r.Id,
            UserId = r.Userid,
            PositionSide = r.PositionSide,
            Symbol = r.Symbol,
            OrderChange = r.OrderChange,
            IsActive = r.IsActive,
            Amount = r.Amount,
            OrderType = r.OrderType,
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
        _redisCacheService.SetCachedData(Constants.RedisAllConfigs, resultDto, TimeSpan.FromDays(100));
        return resultDto;
    }

    public ConfigDto GetById(long id)
    {
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(Constants.RedisAllConfigs);
        if (cachedData != null)
        {
            return cachedData.FirstOrDefault(c => c.Id == id);
        }
        
        return null;
    }

    public List<ConfigDto> GetByUserId(long userid)
    {
        var cachedData = _redisCacheService.GetCachedData<List<ConfigDto>>(Constants.RedisAllConfigs);
        if (cachedData != null)
        {
            return cachedData.Where(c => c.UserId == userid).ToList();
        }
        return new List<ConfigDto>();
    }
}