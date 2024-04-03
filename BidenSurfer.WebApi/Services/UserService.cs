namespace BidenSurfer.WebApi.Services;

using BidenSurfer.Infras;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
public interface IUserService
{
    Task<AuthenticateResponse?> Authenticate(AuthenticateRequest model);
    Task<IEnumerable<UserDto>> GetAll();
    Task<UserDto?> GetById(long id);
    Task<bool> AddOrEdit(UserDto user);
    Task<bool> Delete(long id);
}

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddOrEdit(UserDto user)
    {
        var userEntity = await _context.Users?.FirstOrDefaultAsync(c => c.Id == user.Id);
        if (userEntity == null)
        {
            //Add new
            var userAdd = new User
            {
                FullName = user.FullName,
                Username = user.Username,
                Password = user.Password,
                Role = user.Role,
                Status = user.Status,
                Email = user.Email
            };
            _context.Users.Add(userAdd);
        }
        else
        {
            //Edit
            userEntity.FullName = user.FullName;
            userEntity.Username = user.Username;
            userEntity.Password = user.Password;
            userEntity.Role = user.Role;
            userEntity.Status = user.Status;
            userEntity.Email = user.Email;
            _context.Users.Update(userEntity);
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AuthenticateResponse?> Authenticate(AuthenticateRequest model)
    {
        var user = await _context.Users?.SingleOrDefaultAsync(x => x.Username == model.Username && x.Password == model.Password);

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
        return true;
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
                ApiKey = user.UserSetting.ApiKey,
                SecretKey = user.UserSetting.SecretKey,
                PassPhrase = user.UserSetting.PassPhrase,
                TeleChannel = user.UserSetting.TeleChannel,
                Id = user.UserSetting.Id,
                UserId = user.Id
            }
        };
    }

    // helper methods

    private string generateJwtToken(User user)
    {
        // generate token that is valid for 15 days
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(AppConstants.PrivateKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
            Expires = DateTime.UtcNow.AddDays(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}