using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class ScannerMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; }
        public string Symbol { get; set;}
        public decimal Elastic { get; set;}
        public decimal Volumn { get; set; }
    }
}
