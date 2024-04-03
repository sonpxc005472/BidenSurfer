namespace BidenSurfer.Infras.Entities;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Config
{
    [Key]
    public long Id { get; set; }
    public long Userid { get; set; }
    public string Symbol { get; set; }
    public int OrderType { get; set; }
    public string PositionSide { get; set; }
    public decimal OrderChange { get; set; }         
    public decimal Amount { get; set; }
    public int IncreaseAmountPercent { get; set; }
    public int IncreaseOcPercent { get; set; }
    public int IncreaseAmountExpire { get; set; }
    public decimal AmountLimit { get; set; }   
    public bool IsActive { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
}