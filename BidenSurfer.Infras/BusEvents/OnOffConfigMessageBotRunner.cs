using BidenSurfer.Infras.Models;
using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class OnOffConfigMessageBotRunner : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; }
        public List<ConfigDto> Configs { get; set; }
    }
}
