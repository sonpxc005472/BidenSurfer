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
            var allconfigs = _redisCacheService.GetCachedData<List<ConfigDto>>(AppConstants.RedisAllConfigs);
            var newScan = allconfigs?.Where(c => c.isNewScan).ToList();
            if (newScan != null && newScan.Any())
            {
                foreach (var config in newScan)
                {
                    StaticObject.AllConfigs.TryAdd(config.CustomId, config);
                }

                await _botService.SubscribeSticker();
            }                        
        }
    }
}
