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
}

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserDto>> GetAllActive()
    {        
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
        
        return users;
    }
        
}