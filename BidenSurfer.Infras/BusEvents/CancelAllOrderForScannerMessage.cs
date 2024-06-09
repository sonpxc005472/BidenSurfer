using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class CancelAllOrderForScannerMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}
