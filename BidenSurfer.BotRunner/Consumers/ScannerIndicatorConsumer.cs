using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using CryptoExchange.Net.CommonObjects;
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
                    var symbolConfigs = StaticObject.AllConfigs.Where(c => c.Value.Symbol == config.Symbol && c.Value.IsActive).Select(c => c.Value).ToList();
                    var openScanners = symbolConfigs.Where(x => x.CreatedBy == AppConstants.CreatedByScanner && !string.IsNullOrEmpty(x.ClientOrderId)).ToList();

                    bool isExistedScanner = openScanners.Any(x => x.UserId == config.UserId);
                    var existingFilledOrders = StaticObject.FilledOrders.Where(x => x.Value.UserId == config.UserId && x.Value.OrderStatus == 2 && x.Value.Symbol == config.Symbol).Select(r => r.Value).ToList();
                    var sideOrderExisted = symbolConfigs.Any(x => x.UserId == config.UserId && x.PositionSide != config.PositionSide);
                    if (!isExistedScanner && !existingFilledOrders.Any() && !sideOrderExisted)
                    {
                        await _botService.TakePlaceOrder(config, currentPrice.Value);
                    }                    
                }

                await _botService.SubscribeSticker();
            }                        
        }
    }
}
