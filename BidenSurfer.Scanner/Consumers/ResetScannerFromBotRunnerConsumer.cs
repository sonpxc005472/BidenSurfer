using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Scanner;
using BidenSurfer.Scanner.Services;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BidenSurfer.BotRunner.Consumers
{
    public class ResetScannerFromBotRunnerConsumer : IConsumer<ResetBotForScannerFromBotRunnerMessage>
    {
        private readonly IBotService _botService;
        private readonly IConfigService _configService;
        private readonly ILogger<ResetScannerFromBotRunnerConsumer> _logger;

        public ResetScannerFromBotRunnerConsumer(IBotService botService, IConfigService configService, ILogger<ResetScannerFromBotRunnerConsumer> logger)
        {
            _botService = botService;
            _configService = configService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ResetBotForScannerFromBotRunnerMessage> context)
        {
            _logger.LogInformation("Resetting scanner...");
            var publicApi = new BybitRestClient();
            var spotSymbols = (await publicApi.V5Api.ExchangeData.GetSpotSymbolsAsync()).Data.List;
            StaticObject.Symbols = spotSymbols.Where(s => (s.MarginTrading == MarginTrading.Both || s.MarginTrading == MarginTrading.UtaOnly) && s.Name.EndsWith("USDT")).ToList();

            await _configService.GetAllActiveAsync();
            await _botService.SubscribeSticker();
        }
    }
}
