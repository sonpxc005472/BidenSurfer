namespace BidenSurfer.Infras.Models;
public class ConfigDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
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
    public string OrderId { get; set; }
    public string ClientOrderId { get; set; }
    public decimal? FilledPrice { get; set; }
    public decimal? FilledQuantity { get; set; }
    public decimal? TPPrice { get; set; }
    public int? OrderStatus { get; set; } //1 - new, 2 - filled and closing
    public UserDto UserDto { get; set; }
}