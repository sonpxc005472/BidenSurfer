namespace BidenSurfer.Infras.Entities;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UserSetting
{
    [Key]
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? ApiKey { get; set; }
    public string? SecretKey { get; set; }
    public string? PassPhrase { get; set; }
    public string? TeleChannel { get; set; }   
    public User? User { get; set; }
}