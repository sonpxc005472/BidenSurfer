using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class SaveNewConfigMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();        
    }
}
