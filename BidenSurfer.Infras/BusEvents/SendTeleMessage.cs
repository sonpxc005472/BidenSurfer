using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class SendTeleMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public string Message { get; set; }
        public string TeleChannel { get; set; }
    }
}
