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
            var volume = context.Message?.Volume;
            if (newScans != null && currentPrice.HasValue && newScans.Any())
            {
                Console.WriteLine($"ScannerIndicatorConsumer: {string.Join(",", newScans.Select(x => $"{x.Symbol} - Active:{x.IsActive} - OC: {x.OrderChange}").ToList())}");
                
                foreach (var config in newScans)
                {
                    //// If volume > 200k, then cancel all orders first to optimize the balance
                    //if (volume >= 200000)
                    //{
                    //    var allActiveConfigs = StaticObject.AllConfigs.Where(x => x.Value.IsActive && !string.IsNullOrEmpty(x.Value.ClientOrderId) && x.Value.Symbol != config.Symbol && x.Value.CreatedBy == AppConstants.CreatedByUser).Select(x => x.Value).ToList();
                    //    foreach (var activeConfig in allActiveConfigs)
                    //    {
                    //        await _botService.CancelOrder(activeConfig, false);
                    //    }
                    //}
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
