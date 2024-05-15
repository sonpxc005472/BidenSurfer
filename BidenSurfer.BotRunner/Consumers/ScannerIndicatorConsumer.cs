using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.BotRunner.Consumers
{
    public class ScannerIndicatorConsumer : IConsumer<NewConfigCreatedMessage>
    {
        private readonly IBotService _botService;
        public ScannerIndicatorConsumer(IBotService botService)
        {
            _botService = botService;
        }
        public async Task Consume(ConsumeContext<NewConfigCreatedMessage> context)
        {
            var newScans = context.Message?.ConfigDtos;
            if (newScans != null && newScans.Any())
            {
                Console.WriteLine($"ScannerIndicatorConsumer: {string.Join(",", newScans.Select(x => $"{x.CustomId} - Active:{x.IsActive} - OC: {x.OrderChange}").ToList())}");
                foreach (var config in newScans)
                {
                    StaticObject.AllConfigs.TryAdd(config.CustomId, config);
                }

                await _botService.SubscribeSticker();
            }                        
        }
    }
}
