using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class RestartBotMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
    }
}
