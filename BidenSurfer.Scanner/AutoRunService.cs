using BidenSurfer.Scanner.Services;
using BidenSurfer.Infras;
using Bybit.Net.Clients;
using Microsoft.Extensions.Hosting;

namespace BidenSurfer.Scanner.Services
{
    public class AutoRunService : BackgroundService
    {
        private readonly IBotService _botService;        
        private readonly IScannerService _scannerService; 
        private readonly IConfigService _configService;   
        private readonly IUserService _userService;

        public AutoRunService(IBotService botService, IScannerService scannerService, IConfigService configService, IUserService userService)
        {
            _botService = botService;   
            _scannerService = scannerService;
            _configService = configService;
            _userService = userService;
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
            await _configService.GetAllActiveAsync();
            await _userService.GetAllActive();
            await _scannerService.GetScannerSettings();
            await _scannerService.GetAll();
            await _botService.SubscribeSticker();
        }
    }
}
