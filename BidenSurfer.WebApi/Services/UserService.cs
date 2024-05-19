namespace BidenSurfer.WebApi.Services;

using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using BidenSurfer.WebApi.Helpers;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using CryptoExchange.Net.Authentication;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
public interface IUserService
{
    Task<AuthenticateResponse?> Authenticate(AuthenticateRequest model);
    Task<IEnumerable<UserDto>> GetAll();
    Task<BalanceDto> GetBalance();
    Task<UserDto?> GetById(long id);
    Task<bool> AddOrEdit(UserDto user);
    Task<bool> Delete(long id);
    Task<UserSettingDto?> GetApiSetting();
    Task<bool> SaveApiSetting(UserSettingDto userSettingDto);
    Task<GeneralSettingDto?> GetGeneralSetting();
    Task<bool> SaveGeneralSetting(GeneralSettingDto generalSettingDto);
    Task<bool> StartStopBot(GeneralSettingDto generalSettingDto);
    Task<decimal> GetMaximumBorrow(string symbol, string orderSide);
    string GenHash(string text);
}

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private ISecurityContextAccessor _securityContextAccessor;
    private IBus _bus;
    public UserService(AppDbContext context, ISecurityContextAccessor securityContextAccessor, IBus bus)
    {
        _context = context;
        _securityContextAccessor = securityContextAccessor;
        _bus = bus;
    }

    public async Task<bool> AddOrEdit(UserDto user)
    {
        var userEntity = await _context.Users?.FirstOrDefaultAsync(c => c.Id == user.Id);
        if (userEntity == null)
        {            
            var hashedPassword = SecurityHelper.GenerateHashedPassword(user.Password);
            //Add new
            var userAdd = new User
            {
                FullName = user.FullName,
                Username = user.Username,
                Password = hashedPassword,
                Role = user.Role,
                Status = 1,
                Email = user.Email
            };
            _context.Users.Add(userAdd);
        }
        else
        {
            //Edit
            userEntity.FullName = user.FullName;
            userEntity.Username = user.Username;
            userEntity.Role = user.Role;
            userEntity.Status = user.Status;
            userEntity.Email = user.Email;
            _context.Users.Update(userEntity);
        }
        await _context.SaveChangesAsync();
        _ = _bus.Send(new UpdateUserForBotRunnerMessage());
        _ = _bus.Send(new UpdateUserForScannerMessage());
        return true;
    }

    public async Task<AuthenticateResponse?> Authenticate(AuthenticateRequest model)
    {
        var hashedPass = SecurityHelper.GenerateHashedPassword(model.Password);
        var user = await _context.Users?.SingleOrDefaultAsync(x => x.Username == model.Username && hashedPass == x.Password);

        // return null if user not found
        if (user == null) return null;

        // authentication successful so generate jwt token
        var token = generateJwtToken(user);

        return new AuthenticateResponse(user, token);
    }

    public async Task<bool> Delete(long id)
    {
        var userEntity = await _context.Users?.FirstOrDefaultAsync(c => c.Id == id);
        if (userEntity == null)
        {
            return false;
        }
        _context.Users.Remove(userEntity);
        await _context.SaveChangesAsync();
        _ = _bus.Send(new UpdateUserForBotRunnerMessage());
        _ = _bus.Send(new UpdateUserForScannerMessage());
        return true;
    }

    public string GenHash(string text)
    {
        return SecurityHelper.GenerateHashedPassword(text);
    }

    public async Task<IEnumerable<UserDto>> GetAll()
    {        
        var result = await _context.Users.Include(u => u.UserSetting).ToListAsync() ?? new List<User>();
        return result.Select(r => new UserDto
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
        });
    }

    public async Task<UserDto?> GetById(long id)
    {
        var user = await _context.Users?.Include(u => u.UserSetting).FirstOrDefaultAsync(x => x.Id == id);
        if(user == null) return null;
        return new UserDto
        {
            FullName = user.FullName,
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            Status = user.Status ?? 0,
            Email = user.Email,
            Setting = new UserSettingDto
            {
                ApiKey = user.UserSetting?.ApiKey,
                SecretKey = user.UserSetting?.SecretKey,
                PassPhrase = user.UserSetting?.PassPhrase,
                TeleChannel = user.UserSetting?.TeleChannel,
                Id = user.UserSetting?.Id ?? 0,
                UserId = user.Id
            }
        };
    }

    public async Task<UserSettingDto?> GetApiSetting()
    {
        var userId = _securityContextAccessor.UserId;
        var user = await _context.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId);
        if (user == null)
        {
            return new UserSettingDto
            {
                ApiKey = string.Empty,
                SecretKey = string.Empty,
                PassPhrase = string.Empty,
                TeleChannel = string.Empty,
                UserId = userId
            };
        }
        return new UserSettingDto
        {
            ApiKey = user?.ApiKey,
            SecretKey = user?.SecretKey,
            PassPhrase = user?.PassPhrase,
            TeleChannel = user?.TeleChannel,
            Id = user.Id,
            UserId = user.UserId
        };
    }

    public async Task<bool> SaveApiSetting(UserSettingDto userSettingDto)
    {
        var userId = _securityContextAccessor.UserId;
        var userEntity = await _context.UserSettings?.FirstOrDefaultAsync(c => c.UserId == userId);
        if (userEntity == null)
        {
            var userSetting = new UserSetting
            {
                ApiKey = userSettingDto.ApiKey,
                SecretKey = userSettingDto.SecretKey,
                PassPhrase = userSettingDto.PassPhrase,
                UserId = userId,
                TeleChannel = userSettingDto.TeleChannel
            };
            _context.UserSettings.Add(userSetting);
        }
        else
        {
            userEntity.ApiKey = userSettingDto.ApiKey;
            userEntity.SecretKey = userSettingDto.SecretKey;
            userEntity.PassPhrase = userSettingDto.PassPhrase;
            userEntity.TeleChannel = userSettingDto.TeleChannel;
            _context.UserSettings?.Update(userEntity);
        }
        await _context.SaveChangesAsync();
        _ = _bus.Send(new UpdateUserForBotRunnerMessage());
        _ = _bus.Send(new UpdateUserForScannerMessage());
        return true;
    }
    public async Task<BalanceDto> GetBalance()
    {
        try
        {
            var userSetting = await GetApiSetting();
            if (userSetting == null || string.IsNullOrEmpty(userSetting?.ApiKey) || string.IsNullOrEmpty(userSetting?.SecretKey))
                return new BalanceDto();
            BybitRestClient api = new BybitRestClient();
            api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey));
            var balance = await api.V5Api.Account.GetBalancesAsync(Bybit.Net.Enums.AccountType.Unified);
            return new BalanceDto
            {
                Total = Math.Round(balance.Data?.List?.FirstOrDefault()?.TotalWalletBalance ?? 0, 0),
                Available = Math.Round(balance.Data?.List?.FirstOrDefault()?.TotalAvailableBalance ?? 0,0)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new BalanceDto();
        }
    }

    // helper methods

    private string generateJwtToken(User user)
    {
        // generate token that is valid for 15 days
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(AppConstants.PrivateKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("userId", user.Id.ToString()), new Claim("role", user.Role.ToString()) }),
            Expires = DateTime.UtcNow.AddDays(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<decimal> GetMaximumBorrow(string symbol, string orderSide)
    {
        try
        {
            var userSetting = await GetApiSetting();
            if (userSetting == null || string.IsNullOrEmpty(userSetting?.ApiKey) || string.IsNullOrEmpty(userSetting?.SecretKey))
                return 0;
            BybitRestClient api = new BybitRestClient();
            api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey));
            var orderSideConvert = orderSide.ToLower() == AppConstants.LongSide ? OrderSide.Buy : OrderSide.Sell;
            var info = await api.V5Api.Trading.GetBorrowQuotaAsync(Bybit.Net.Enums.Category.Spot, symbol, orderSideConvert);
            if(info.Success)
            {
                return Math.Round(info.Data.MaxTradeAmount,0);
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 0;
        }
    }

    public async Task<GeneralSettingDto?> GetGeneralSetting()
    {
        var userId = _securityContextAccessor.UserId;
        var setting = await _context.GeneralSettings.FirstOrDefaultAsync(x => x.UserId == userId);
        if (setting == null)
        {
            return new GeneralSettingDto
            {
                Userid = userId,
                Budget = 0,
                AssetTracking = 0,
                Id = 0,
                Stop = false
            };
        }
        return new GeneralSettingDto
        {
            Id = setting.Id,
            Budget = setting?.Budget,
            AssetTracking = setting?.AssetTracking,
            Userid = setting.UserId,
            Stop = setting?.Stop
        };
    }

    public async Task<bool> SaveGeneralSetting(GeneralSettingDto generalSettingDto)
    {
        var userId = _securityContextAccessor.UserId;
        var setting = await _context.GeneralSettings?.FirstOrDefaultAsync(c => c.UserId == userId);
        if (setting == null)
        {
            var userSetting = new GeneralSetting
            {
                UserId = userId,
                Budget = generalSettingDto.Budget,
                AssetTracking = generalSettingDto.AssetTracking,
                Stop = false
            };
            _context.GeneralSettings.Add(userSetting);
        }
        else
        {
            setting.Budget = generalSettingDto.Budget;
            setting.AssetTracking = generalSettingDto.AssetTracking;
            _context.GeneralSettings?.Update(setting);
        }
        await _context.SaveChangesAsync();
       
        return true;
    }

    public async Task<bool> StartStopBot(GeneralSettingDto generalSettingDto)
    {
        var userId = _securityContextAccessor.UserId;
        var setting = await _context.GeneralSettings?.FirstOrDefaultAsync(c => c.UserId == userId);
        if (setting != null && setting.Stop != generalSettingDto.Stop)
        {
            setting.Stop = generalSettingDto.Stop;
            _context?.GeneralSettings?.Update(setting);
            await _context.SaveChangesAsync();
            _ = _bus.Send(new StartStopBotForBotRunnerMessage
            {
                UserId = userId,
                IsStop = generalSettingDto.Stop.Value
            });
            _ = _bus.Send(new StartStopBotForScannerMessage
            {
                UserId = userId,
                IsStop = generalSettingDto.Stop.Value
            });
        }
        return true;
    }    
}