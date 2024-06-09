using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BidenSurfer.Scanner.Consumers
{
    public class CancelAllOrderConsumer : IConsumer<CancelAllOrderForScannerMessage>
    {
        private readonly IConfigService _configService;
        private readonly ILogger<CancelAllOrderConsumer> _logger;
        public CancelAllOrderConsumer(IConfigService configService, ILogger<CancelAllOrderConsumer> logger)
        {
            _configService = configService;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<CancelAllOrderForScannerMessage> context)
        {
            _logger.LogInformation($"CancelAllOrderConsumer...");
            var activeConfig = StaticObject.AllConfigs.Where(x=>x.Value.IsActive).Select(x => x.Value).ToList();
            _configService.AddOrEditConfig(activeConfig);
            await Task.CompletedTask;
        }
    }
}
