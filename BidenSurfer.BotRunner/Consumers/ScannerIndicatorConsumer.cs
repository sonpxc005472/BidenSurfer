using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Models;
using MassTransit;

namespace BidenSurfer.BotRunner.Consumers
{
    public class ScannerIndicatorConsumer : IConsumer<NewConfigCreatedMessage>
    {
        private readonly IBotService _botService;
        private readonly IRedisCacheService _redisCacheService;
        public ScannerIndicatorConsumer(IBotService botService, IRedisCacheService redisCacheService)
        {
            _botService = botService;
            _redisCacheService = redisCacheService;
        }
        public async Task Consume(ConsumeContext<NewConfigCreatedMessage> context)
        {
            var newScans = context.Message?.ConfigDtos;
            if (newScans != null && newScans.Any())
            {
                foreach (var config in newScans)
                {
                    StaticObject.AllConfigs.TryAdd(config.CustomId, config);
                }

                await _botService.SubscribeSticker();
            }                        
        }
    }
}
