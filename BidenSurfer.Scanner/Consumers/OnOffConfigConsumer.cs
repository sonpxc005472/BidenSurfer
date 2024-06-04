using BidenSurfer.Infras.BusEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BidenSurfer.Scanner.Consumers
{
    public class OnOffConfigConsumer : IConsumer<OnOffConfigMessageScanner>
    {
        private readonly IConfigService _configService;
        private readonly ILogger<OnOffConfigConsumer> _logger;
        public OnOffConfigConsumer(IConfigService configService, ILogger<OnOffConfigConsumer> logger)
        {
            _configService = configService;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<OnOffConfigMessageScanner> context)
        {
            _logger.LogInformation($"OnOffConfigConsumer {string.Join(",", context.Message.Configs.Select(c => c.CustomId).ToList())}");
            _configService.OnOffConfig(context.Message.Configs);
            await Task.CompletedTask;
        }
    }
}
