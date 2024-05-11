using BidenSurfer.Infras.Models;
using MassTransit;

namespace BidenSurfer.Infras.BusEvents
{
    public class ScannerSettingUpdateFromApiMessage : CorrelatedBy<Guid>   
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public ScannerSettingDto? ScannerSettingDto { get; set; }
        public bool IsDelete { get; set; } = false;
    }
}
