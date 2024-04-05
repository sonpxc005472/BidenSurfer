using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class OffConfigMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; }
        List<string> CustomIds { get; set; }
    }
}
