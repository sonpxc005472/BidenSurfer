using BidenSurfer.Infras.Models;
using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class ConfigUpdateFromApiForBotRunnerMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public List<ConfigDto>? ConfigDtos { get; set; }
        public bool IsDelete { get; set; } = false;
    }
}
