using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class ResetBotForScannerMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}
