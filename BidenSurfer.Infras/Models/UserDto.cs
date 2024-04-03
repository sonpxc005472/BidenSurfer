namespace BidenSurfer.Infras.Models;
public class UserDto
{
    public long Id { get; set; }
    public string? FullName { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public int Role { get; set; }
    public int Status { get; set; }
    public UserSettingDto? Setting { get; set; }
}