﻿using BidenSurfer.Scanner.Services;
using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.Scanner.Consumers
{
    public class ScannerUpdateFromApiConsumer : IConsumer<ScannerUpdateFromApiMessage>
    {
        private readonly IScannerService _scannerService;
        public ScannerUpdateFromApiConsumer(IScannerService scannerService)
        {
            _scannerService = scannerService;
        }
        public async Task Consume(ConsumeContext<ScannerUpdateFromApiMessage> context)
        {
            var scanners = context.Message?.ScannerDtos;
            if (scanners != null && scanners.Any())
            {
                _scannerService.AddOrEdit(scanners);
            }                        
        }
    }
}
