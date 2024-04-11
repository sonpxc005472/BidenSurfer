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
            Console.WriteLine("Running...");
            _configService.DeleteAllConfig();
            _userService.DeleteAllCached();
            await _userService.GetAllActive();
            await _configService.GetAllActive();
            await _botService.InitUserApis();
            await _botService.SubscribeKline1m();
            await _botService.SubscribeSticker();
            await _botService.SubscribeOrderChannel();
        }
    }
}
