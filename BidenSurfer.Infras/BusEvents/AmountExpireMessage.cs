using BidenSurfer.Infras.Models;
using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class AmountExpireMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public List<string> Configs { get; set; }
    }
}
