namespace BidenSurfer.Infras.Entities;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Scanner
{
    [Key]
    public long Id { get; set; }
    public long Userid { get; set; }
    public string Title { get; set; }
    public int OrderType { get; set; }
    public int Elastic { get; set; }
    public string PositionSide { get; set; }
    public decimal OrderChange { get; set; }         
    public decimal Turnover { get; set; }         
    public decimal Amount { get; set; }
    public List<string>? OnlyPairs { get; set; }
    public List<string>? BlackList { get; set; }
    public int OcNumber { get; set; }
    public int AmountExpire { get; set; }
    public int ConfigExpire { get; set; }
    public int AutoAmount { get; set; }
    public decimal AmountLimit { get; set; }   
    public bool IsActive { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
}