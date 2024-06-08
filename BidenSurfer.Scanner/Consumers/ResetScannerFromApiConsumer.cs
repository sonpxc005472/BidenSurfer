using BidenSurfer.Infras.BusEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BidenSurfer.Scanner.Consumers
{
    public class ResetScannerFromApiConsumer : IConsumer<ResetBotForScannerFromApiMessage>
    {
        private readonly IConfigService _configService;
        private readonly ILogger<ResetScannerFromApiConsumer> _logger;
        public ResetScannerFromApiConsumer(IConfigService configService, ILogger<ResetScannerFromApiConsumer> logger)
        {
            _configService = configService;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<ResetBotForScannerFromApiMessage> context)
        {
            _logger.LogInformation($"Reset bot");
            //_configService.OnOffConfig(context.Message.Configs);
            await Task.CompletedTask;
        }
    }
}
