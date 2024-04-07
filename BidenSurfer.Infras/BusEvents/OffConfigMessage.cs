using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class OffConfigMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; }
        public List<string> CustomIds { get; set; }
    }
}
