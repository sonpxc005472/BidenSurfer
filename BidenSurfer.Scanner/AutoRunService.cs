using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras;
using Bybit.Net.Clients;
using Microsoft.Extensions.Hosting;

namespace BidenSurfer.Scanner.Services
{
    public class AutoRunService : BackgroundService
    {
        private readonly IBotService _botService;        
        private readonly IScannerService _scannerService;        

        public AutoRunService(IBotService botService, IScannerService scannerService)
        {
            _botService = botService;   
            _scannerService = scannerService;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Running...");
            //var chatId = "-1001847838569"; // Thay thế bằng ID kênh của bạn
            //var text = "<b>Running</b>\n<b>PNL</b>: 100%"; // Thay thế bằng nội dung tin nhắn bạn muốn gửi

            //await TelegramHelper.SendMessage(text, chatId);
            if (!StaticObject.Symbols.Any())
            {
                var publicApi = new BybitRestClient();
                var spotSymbols = (await publicApi.V5Api.ExchangeData.GetSpotSymbolsAsync()).Data.List;
                StaticObject.Symbols = spotSymbols.ToList();
            }
            _scannerService.DeleteAll();
            await _scannerService.GetAll();
            await _botService.SubscribeSticker();
        }
    }
}
