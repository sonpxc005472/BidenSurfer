namespace BidenSurfer.BotRunner.Services;

using BidenSurfer.Infras;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public interface IUserService
{ 
    Task<List<UserDto>> GetAllActive();
    void DeleteAllCached();
    void SaveSymbolCollateral(long userId, List<string> symbolUser);
    List<string> GetSymbolCollateral(long userId);
    Task<GeneralSettingDto?> GetGeneralSetting(long userId);
}

public class UserService : IUserService
{
    private readonly IRedisCacheService _redisCacheService;
    private readonly AppDbContext _dbContext;

    public UserService(IRedisCacheService redisCacheService, AppDbContext dbContext)
    {
        _redisCacheService = redisCacheService;
        _dbContext = dbContext;
    }

    public void DeleteAllCached()
    {
        _redisCacheService.RemoveCachedData(AppConstants.RedisAllUsers);
    }

    public async Task<List<UserDto>> GetAllActive()
    {
        Console.WriteLine("GetAllActive - db");

        var result = (await _dbContext?.Users?.Include(u => u.UserSetting).Where(u => u.Status == (int) UserStatusEnums.Active && u.UserSetting != null).ToListAsync()) ?? new List<User>();

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

    public async Task<GeneralSettingDto?> GetGeneralSetting(long userId)
    {
        var setting = await _dbContext.GeneralSettings.FirstOrDefaultAsync(x => x.UserId == userId);
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
        return new GeneralSettingDto
        {
            Id = setting.Id,
            Budget = setting?.Budget,
            AssetTracking = setting?.AssetTracking,
            Userid = setting.UserId
        };
    }

    public List<string> GetSymbolCollateral(long userId)
    {
        var cachedData = _redisCacheService.GetCachedData<Dictionary<long,List<string>>>(AppConstants.RedisSymbolCollateral);
        if (cachedData != null && cachedData.ContainsKey(userId))
        {
            return cachedData[userId];
        }
        return new List<string>();
    }

    public void SaveSymbolCollateral(long userId, List<string> symbolUser)
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
            cachedData = new Dictionary<long, List<string>> { { userId, symbolUser}};
        }
        _redisCacheService.SetCachedData(AppConstants.RedisSymbolCollateral, cachedData, TimeSpan.FromDays(1000));
    }
}