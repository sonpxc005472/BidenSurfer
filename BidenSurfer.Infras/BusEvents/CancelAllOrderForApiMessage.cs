using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class CancelAllOrderForApiMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}
