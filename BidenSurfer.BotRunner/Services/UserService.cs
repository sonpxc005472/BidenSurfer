namespace BidenSurfer.BotRunner.Services;

using BidenSurfer.Infras;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

public interface IUserService
{
    Task GetBotStatus();
    Task<List<UserDto>> GetAllActive();
    void DeleteAllCached();
    void SaveSymbolCollateral(long userId, List<string> symbolUser);
    List<string> GetSymbolCollateral(long userId);
    Task<GeneralSettingDto?> GetGeneralSetting(long userId);
}

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IRedisCacheService _redisCacheService;
    private readonly AppDbContext _dbContext;

    public UserService(ILogger<UserService> logger, IRedisCacheService redisCacheService, AppDbContext dbContext)
    {
        _logger = logger;
        _redisCacheService = redisCacheService;
        _dbContext = dbContext;
    }

    public void DeleteAllCached()
    {
        try
        {
            _redisCacheService.RemoveCachedData(AppConstants.RedisAllUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while deleting cached data: {ex.Message}");
        }
    }

    public async Task<List<UserDto>> GetAllActive()
    {
        try
        {
            _logger.LogInformation("GetAllActive - db");

            var result = (await _dbContext?.Users.AsNoTracking().Include(u => u.UserSetting).Where(u => u.Status == (int)UserStatusEnums.Active && u.UserSetting != null).ToListAsync()) ?? new List<User>();

            var users = result.Select(r => new UserDto
            {
                FullName = r.FullName,
                Id = r.Id,
                Username = r.Username,
                Password = r.Password,
                Role = r.Role,
                Email = r.Email,
                Status = r.Status ?? 0,
                Setting = new UserSettingDto
                {
                    ApiKey = r.UserSetting.ApiKey,
                    SecretKey = r.UserSetting.SecretKey,
                    PassPhrase = r.UserSetting.PassPhrase,
                    TeleChannel = r.UserSetting.TeleChannel,
                    Id = r.UserSetting.Id,
                    UserId = r.Id
                }
            }).ToList();
            StaticObject.AllUsers = users;
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while getting all active users: {ex.Message}");
            return new List<UserDto>();
        }
    }

    public async Task GetBotStatus()
    {
        try
        {
            var result = await _dbContext?.GeneralSettings.AsNoTracking().ToListAsync();

            StaticObject.BotStatus = result.Select(r => new
            {
                r.UserId,
                r.Stop
            }).Distinct().ToDictionary(r => r.UserId, r => r.Stop.HasValue ? !r.Stop.Value : true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while getting bot status: {ex.Message}");
        }
    }

    public async Task<GeneralSettingDto?> GetGeneralSetting(long userId)
    {
        try
        {
            var cachedData = _redisCacheService.GetCachedData<GeneralSettingDto>(AppConstants.RedisGeneralSettings);

            if (cachedData != null)
            {
                return cachedData;
            }

            var setting = await _dbContext.GeneralSettings.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
            if (setting == null)
            {
                return new GeneralSettingDto
                {
                    Userid = userId,
                    Budget = 0,
                    AssetTracking = 0,
                    Id = 0
                };
            }
            var generalSettingDto = new GeneralSettingDto
            {
                Id = setting.Id,
                Budget = setting?.Budget,
                AssetTracking = setting?.AssetTracking,
                Userid = setting.UserId
            };
            _redisCacheService.SetCachedData(AppConstants.RedisGeneralSettings, generalSettingDto, TimeSpan.FromDays(100));
            return generalSettingDto;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while getting general setting: {ex.Message}");
            return null;
        }
    }

    public List<string> GetSymbolCollateral(long userId)
    {
        try
        {
            var cachedData = _redisCacheService.GetCachedData<Dictionary<long, List<string>>>(AppConstants.RedisSymbolCollateral);
            if (cachedData != null && cachedData.ContainsKey(userId))
            {
                return cachedData[userId];
            }
            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while getting symbol collateral: {ex.Message}");
            return new List<string>();
        }
    }

    public void SaveSymbolCollateral(long userId, List<string> symbolUser)
    {
        try
        {
            var cachedData = _redisCacheService.GetCachedData<Dictionary<long, List<string>>>(AppConstants.RedisSymbolCollateral);
            if (cachedData != null)
            {
                if (cachedData.ContainsKey(userId))
                {
                    var userCollateral = cachedData[userId];
                    userCollateral.AddRange(symbolUser);
                    cachedData[userId] = userCollateral;
                }
                else
                {
                    cachedData.Add(userId, symbolUser);
                }

            }
            else
            {
                cachedData = new Dictionary<long, List<string>> { { userId, symbolUser } };
            }
            _redisCacheService.SetCachedData(AppConstants.RedisSymbolCollateral, cachedData, TimeSpan.FromDays(1000));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while saving symbol collateral: {ex.Message}");
        }
    }
}
