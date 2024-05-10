using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class UpdateUserForScannerMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}
