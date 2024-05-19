using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class StartStopBotForScannerMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public long UserId { get; set; }
        public bool IsStop { get;set; }
    }
}
