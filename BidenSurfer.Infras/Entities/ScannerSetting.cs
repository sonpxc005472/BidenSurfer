namespace BidenSurfer.Infras.Entities;

using System.ComponentModel.DataAnnotations;

public class ScannerSetting
{
    [Key]
    public long Id { get; set; }
    public long Userid { get; set; }
    public List<string>? BlackList { get; set; }
    public int MaxOpen { get; set; }
    public bool? Stop { get; set; }

}