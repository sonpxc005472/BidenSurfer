using BidenSurfer.Infras.Models;
using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class UpdateConfigMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public List<ConfigDto> Configs { get; set; }
    }
}
