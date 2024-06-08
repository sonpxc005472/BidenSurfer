using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class ResetBotForScannerFromApiMessage : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}
