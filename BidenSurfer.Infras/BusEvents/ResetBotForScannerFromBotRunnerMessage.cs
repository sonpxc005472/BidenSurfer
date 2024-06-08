using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class ResetBotForScannerFromBotRunnerMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}
