using BidenSurfer.Infras;
using Bybit.Net.Clients;
using Microsoft.Extensions.Hosting;

namespace BidenSurfer.BotRunner.Services
{
    public class AutoRunBotService : BackgroundService
    {
        private readonly IBotService _botService;
        private readonly IConfigService _configService;
        private readonly IUserService _userService;
        public AutoRunBotService(IBotService botService, IConfigService configService, IUserService userService)
        {
            _botService = botService;
            _configService = configService;
            _userService = userService;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Starting...");
                StaticObject.TickerSubscriptions.Clear();
                StaticObject.FilledOrders.Clear();
                var socketClient = BybitSocketClientSingleton.Instance;
                await socketClient.UnsubscribeAllAsync();
                _configService.DeleteAllConfig();
                _userService.DeleteAllCached();
                await _userService.GetAllActive();
                await _userService.GetBotStatus();
                await _configService.GetAllActive();
                await _botService.InitUserApis();
                await _botService.CancelAllOrder();
                await _botService.SubscribeKline1m();
                await _botService.SubscribeSticker();
                await _botService.SubscribeOrderChannel();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            
        }
    }
}
