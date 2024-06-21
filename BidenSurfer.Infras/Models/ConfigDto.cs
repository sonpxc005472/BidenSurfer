using BidenSurfer.Infras.Entities;
using System;

namespace BidenSurfer.Infras.Models;
public class ConfigDto
{
    public long Id { get; set; }
    public string CustomId { get; set; }
    public long UserId { get; set; }
    public string Symbol { get; set; }
    public int OrderType { get; set; }
    public string PositionSide { get; set; }
    public decimal OrderChange { get; set; }
    
    public decimal Amount { get; set; }
    public decimal? OriginAmount { get; set; }
    public int? IncreaseAmountPercent { get; set; }
    public int? IncreaseOcPercent { get; set; }
    public int? IncreaseAmountExpire { get; set; }
    public int? Expire { get; set; }
    public decimal? AmountLimit { get; set; }
    public string CreatedBy { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? EditedDate { get; set; }
    public string OrderId { get; set; }
    public string ClientOrderId { get; set; }
    public decimal? FilledPrice { get; set; }
    public decimal? FilledQuantity { get; set; }
    public decimal? TotalQuantity { get; set; }
    public decimal? TPPrice { get; set; }
    public bool isNewScan { get; set; }
    public int? OrderStatus { get; set; } //1 - new, 2 - filled and closing
    public bool isClosingFilledOrder { get; set; }
    public bool isNotTryTP { get; set; }
    public string ScannerTitle { get; set; }
    public decimal? TotalFee { get; set; }
    public DateTime? Timeout { get; set; }
    public UserDto UserDto { get; set; }
    public ConfigDto Clone()
    {
        return (ConfigDto)MemberwiseClone();
    }
}

public static class ConfigHelper
{
    public static ConfigDto ToDto(this Config config)
    {
        return new ConfigDto
        {
            Amount = config.Amount,
            AmountLimit = config.AmountLimit,
            Id = config.Id,
            IncreaseAmountExpire = config.IncreaseAmountExpire,
            IncreaseAmountPercent = config.IncreaseAmountPercent,
            IncreaseOcPercent = config.IncreaseOcPercent,
            IsActive = config.IsActive,
            OrderChange = config.OrderChange,
            OrderType = config.OrderType,
            PositionSide = config.PositionSide,
            Symbol = config.Symbol,
            UserId = config.Userid,
            CustomId = config.CustomId,
            OriginAmount = config.OriginAmount,
            CreatedBy = config.CreatedBy,
            CreatedDate = config.CreatedDate,
            EditedDate = config.EditedDate,
            Expire = config.Expire
        };
    }
}