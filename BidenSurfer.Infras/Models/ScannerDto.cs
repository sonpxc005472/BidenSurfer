using BidenSurfer.Infras.Entities;

namespace BidenSurfer.Infras.Models;
public class ScannerDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Title { get; set; }
    public int OrderType { get; set; }
    public int Elastic { get; set; }
    public int OcNumber { get; set; }
    public decimal Turnover { get; set; }
    public string PositionSide { get; set; }
    public decimal OrderChange { get; set; }    
    public decimal Amount { get; set; }
    public decimal AmountLimit { get; set; }
    public int AmountExpire { get; set; }
    public int AutoAmount { get; set; }
    public int ConfigExpire { get; set; }
    public List<string>? OnlyPairs { get; set; }
    public List<string>? BlackList { get; set; }
    public bool IsActive { get; set; }    
}

public static class ScannerHelper
{
    public static ScannerDto ToDto(this Scanner scanner)
    {
        return new ScannerDto
        {
            Amount = scanner.Amount,
            AmountLimit = scanner.AmountLimit,
            Id = scanner.Id,
            AmountExpire = scanner.AmountExpire,
            AutoAmount = scanner.AutoAmount,
            BlackList = scanner.BlackList,
            OnlyPairs = scanner.OnlyPairs,
            OrderChange = scanner.OrderChange,
            OrderType = scanner.OrderType,
            PositionSide = scanner.PositionSide,
            Title = scanner.Title,
            UserId = scanner.Userid,
            ConfigExpire = scanner.ConfigExpire,
            Elastic = scanner.Elastic,
            OcNumber = scanner.OcNumber,
            Turnover = scanner.Turnover,
        };
    }
}