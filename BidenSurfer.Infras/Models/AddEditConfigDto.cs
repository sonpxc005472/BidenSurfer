using BidenSurfer.Infras.Entities;

namespace BidenSurfer.Infras.Models;
public class AddEditConfigDto
{
    public long Id { get; set; }
    public string Symbol { get; set; }
    public string? CustomId { get; set; }
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
    public bool IsActive { get; set; }
}
