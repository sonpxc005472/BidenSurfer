using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class RunAllBotMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; }
    }
}
