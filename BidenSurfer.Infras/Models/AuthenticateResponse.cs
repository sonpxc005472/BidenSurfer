namespace BidenSurfer.Infras.Models;

using BidenSurfer.Infras.Entities;

public class AuthenticateResponse
{
    public UserDto User { get; set; }
    public string Token { get; set; }


    public AuthenticateResponse(User user, string token)
    {
        User = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
        Token = token;
    }
}