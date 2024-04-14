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
            //var allConfigs = await _configService.GetAllActive();
            //await _botService.SubscribeKline1m();
            //await _botService.SubscribeSticker();
            //var count = allConfigs.Count();
        }
    }
}
