using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Scanner.Services;
using MassTransit;

namespace BidenSurfer.Scanner.Consumers
{
    public class ScannerSettingUpdateFromApiConsumer : IConsumer<ScannerSettingUpdateFromApiMessage>
    {
        private readonly IScannerService _scannerService;
        public ScannerSettingUpdateFromApiConsumer(IScannerService scannerService)
        {
            _scannerService = scannerService;
        }
        public async Task Consume(ConsumeContext<ScannerSettingUpdateFromApiMessage> context)
        {
            Console.WriteLine("ScannerSettingUpdateFromApiConsumer...");
            await _scannerService.GetScannerSettings();
        }
    }
}
