using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.BotRunner.Consumers
{
    public class ScannerIndicatorConsumer : IConsumer<NewConfigCreatedMessage>
    {
        private readonly IBotService _botService;
        private readonly IConfigService _configService;
        public ScannerIndicatorConsumer(IBotService botService, IConfigService configService)
        {
            _botService = botService;
            _configService = configService;
        }
        public async Task Consume(ConsumeContext<NewConfigCreatedMessage> context)
        {
            //var allConfigs = await _configService.GetAllActive();
            //await _botService.SubscribeKline1m();
            await _botService.SubscribeSticker();
            //var count = allConfigs.Count();
        }
    }
}
