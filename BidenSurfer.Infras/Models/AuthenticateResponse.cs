namespace BidenSurfer.Infras.Models;

using BidenSurfer.Infras.Entities;

public class AuthenticateResponse
{
    public long Id { get; set; }
    public string? FullName { get; set; }
    public string? Username { get; set; }
    public int Role { get; set; }
    public string Token { get; set; }


    public AuthenticateResponse(User user, string token)
    {
        Id = user.Id;
        FullName = user.FullName;
        Username = user.Username;
        Role = user.Role;  
        Token = token;
    }
}