using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.Scanner.Consumers
{
    public class OnOffConfigConsumer : IConsumer<OnOffConfigMessageScanner>
    {
        private readonly IConfigService _configService;
        public OnOffConfigConsumer(IConfigService configService)
        {
            _configService = configService;
        }
        public async Task Consume(ConsumeContext<OnOffConfigMessageScanner> context)
        {
            Console.WriteLine($"OnOffConfigConsumer {string.Join(",", context.Message.Configs.Select(c => c.CustomId).ToList())}");
            _configService.OnOffConfig(context.Message.Configs);
            await Task.CompletedTask;
        }
    }
}
