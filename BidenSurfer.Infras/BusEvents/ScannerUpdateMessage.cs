using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class ScannerUpdateMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}
