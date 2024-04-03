using BidenSurfer.Infras;
using Bybit.Net.Clients;
using Microsoft.Extensions.Hosting;

namespace BidenSurfer.Scanner.Services
{
    public class AutoRunService : BackgroundService
    {
        private readonly IBotService _botService;        

        public AutoRunService(IBotService botService)
        {
            _botService = botService;           
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Running...");
            if (!StaticObject.Symbols.Any())
            {
                var publicApi = new BybitRestClient();
                var spotSymbols = (await publicApi.V5Api.ExchangeData.GetSpotSymbolsAsync()).Data.List;
                StaticObject.Symbols = spotSymbols.ToList();
            }
            await _botService.SubscribeSticker();
        }
    }
}
