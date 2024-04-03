namespace BidenSurfer.Infras.Models;
public class UserSettingDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string ApiKey { get; set; }
    public string SecretKey { get; set; }
    public string TeleChannel { get; set; }   
    public string PassPhrase { get; set; }   
}