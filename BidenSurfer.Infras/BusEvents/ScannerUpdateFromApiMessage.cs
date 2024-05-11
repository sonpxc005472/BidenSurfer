using BidenSurfer.Infras.Models;
using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class ScannerUpdateFromApiMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public List<ScannerDto>? ScannerDtos { get; set; }
        public bool IsDelete { get; set; } = false;
    }
}
