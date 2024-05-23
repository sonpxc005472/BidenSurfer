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
            var currentPrice = context.Message?.Price;
            if (newScans != null && currentPrice.HasValue && newScans.Any())
            {
                Console.WriteLine($"ScannerIndicatorConsumer: {string.Join(",", newScans.Select(x => $"{x.Symbol} - Active:{x.IsActive} - OC: {x.OrderChange}").ToList())}");
                foreach (var config in newScans)
                {
                    await _botService.TakePlaceOrder(config, currentPrice.Value);
                }
                // get all keys of the dictionary StaticObject.TickerSubscriptions
                var symbols = StaticObject.TickerSubscriptions.Keys.ToList();
                if (newScans.Any(x=> !symbols.Contains(x.Symbol)))
                {
                    await _botService.SubscribeSticker();
                }
            }                        
        }
    }
}
