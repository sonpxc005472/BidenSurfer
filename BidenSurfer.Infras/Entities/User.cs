namespace BidenSurfer.Infras.Entities;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class User
{
    [Key]
    public long Id { get; set; }
    public string? FullName { get; set; }
    public string? Username { get; set; }
    [JsonIgnore]
    public string? Password { get; set; }
    public string? Email { get; set; }
    public int Role { get; set; }
    public int? Status { get; set; }
    [JsonIgnore]
    public UserSetting? UserSetting { get; set; }
    [JsonIgnore]
    public List<Config>? Configs { get; set; }
    [JsonIgnore]
    public List<Scanner>? Scanners { get; set; }
}