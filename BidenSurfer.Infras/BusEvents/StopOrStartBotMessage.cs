using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class StopOrStartBotMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public Guid BotId { get; set; }
        public bool IsStarted { get; set; }
    }
}
