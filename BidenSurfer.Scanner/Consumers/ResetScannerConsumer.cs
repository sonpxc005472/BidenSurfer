using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Scanner.Services;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BidenSurfer.Scanner.Consumers
{
    public class ResetScannerConsumer : IConsumer<ResetBotForScannerMessage>
    {
        private readonly IBotService _botService;
        private readonly IConfigService _configService;
        private readonly ILogger<ResetScannerConsumer> _logger;
        public ResetScannerConsumer(IBotService botService, IConfigService configService, ILogger<ResetScannerConsumer> logger)
        {
            _botService = botService;
            _configService = configService;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<ResetBotForScannerMessage> context)
        {
            _logger.LogInformation("Resetting scanner...");
            var publicApi = new BybitRestClient();
            var spotSymbols = (await publicApi.V5Api.ExchangeData.GetSpotSymbolsAsync()).Data.List;
            StaticObject.Symbols = spotSymbols.Where(s => (s.MarginTrading == MarginTrading.Both || s.MarginTrading == MarginTrading.UtaOnly) && s.Name.EndsWith("USDT")).ToList();
            var socketClient = BybitSocketClientSingleton.Instance;
            StaticObject.ScannerTradeSubscriptions.Clear();
            await socketClient.UnsubscribeAllAsync();
            await _configService.GetAllActiveAsync();
            await _botService.SubscribeSticker();
        }
    }
}
