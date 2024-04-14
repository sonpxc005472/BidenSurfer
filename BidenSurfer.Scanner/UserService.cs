namespace BidenSurfer.Scanner;

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
        List<UserDto> resultDto = new List<UserDto>();
        var cachedData = _redisCacheService.GetCachedData<List<UserDto>>(AppConstants.RedisAllUsers);
        if (cachedData != null)
        {
            var activeUsers = cachedData.Where(c => c.Status == (int)UserStatusEnums.Active).ToList();
            StaticObject.AllUsers = activeUsers;
            return activeUsers;
        }
        var result = (await _dbContext?.Users?.Include(u => u.UserSetting).Where(u => u.Status == (int) UserStatusEnums.Active).ToListAsync()) ?? new List<User>();

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
        _redisCacheService.SetCachedData(AppConstants.RedisAllUsers, users, TimeSpan.FromDays(100));
        return users;
    }
        
}