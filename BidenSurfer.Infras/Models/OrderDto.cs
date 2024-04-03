namespace BidenSurfer.Infras.Models;
public class OrderDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string ExternalOrderId { get; set; }
    public string Symbol { get; set; }
    public string PositionSide { get; set; }
    public string CandleStick { get; set; }
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public decimal? TpPrice { get; set; }
    public decimal? FillPrice { get; set; }
    public decimal? Quantity { get; set; }
}