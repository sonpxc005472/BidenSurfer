using BidenSurfer.Infras.Models;
using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class NewConfigCreatedMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; }
        public decimal? Price { get; set;}
        public List<ConfigDto>? ConfigDtos { get; set;}
    }
}
