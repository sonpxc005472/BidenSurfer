using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BidenSurfer.Scanner.Consumers
{
    public class StartStopScannerConsumer : IConsumer<StartStopScannerMessage>
    {
        private readonly ILogger<StartStopBotConsumer> _logger;
        public StartStopScannerConsumer(ILogger<StartStopBotConsumer> logger)
        {
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<StartStopScannerMessage> context)
        {
            _logger.LogInformation("Start/Stop scanner...");
            var userId = context.Message.UserId;
            var isStop = context.Message.IsStop;
            if (StaticObject.ScannerStatus.ContainsKey(userId))
            {
                StaticObject.ScannerStatus[userId] = !isStop;
            }
            else
            {
                StaticObject.ScannerStatus.Add(userId, !isStop);
            }                       
        }
    }
}
