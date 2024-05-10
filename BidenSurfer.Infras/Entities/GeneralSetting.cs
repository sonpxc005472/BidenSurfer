namespace BidenSurfer.Infras.Entities;

using System.ComponentModel.DataAnnotations;

public class GeneralSetting
{
    [Key]
    public long Id { get; set; }
    public long Userid { get; set; }
    public decimal? Budget { get; set; }
    public decimal? AssetTracking { get; set; }    
}