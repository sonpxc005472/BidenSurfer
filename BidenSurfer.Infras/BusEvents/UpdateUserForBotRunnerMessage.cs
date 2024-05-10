using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class UpdateUserForBotRunnerMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}
