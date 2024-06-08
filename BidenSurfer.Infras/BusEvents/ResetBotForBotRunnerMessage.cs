using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class ResetBotForBotRunnerMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}
