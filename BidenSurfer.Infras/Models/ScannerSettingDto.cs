namespace BidenSurfer.Infras.Models;
public class ScannerSettingDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public int MaxOpen { get; set; }
    public List<string>? BlackList { get; set; }
}