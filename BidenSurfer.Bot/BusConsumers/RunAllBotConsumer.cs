using MassTransit;
using BidenSurfer.Bot.Services;
using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;

namespace BidenSurfer.Bot.BusConsumers
{
    public class RunAllBotConsumer : IConsumer<RunAllBotMessage>
    {
        private readonly IBotService _botService;
        private readonly IConfigService _configService;
        public RunAllBotConsumer(IBotService botService, IConfigService configService)
        {            
            _botService = botService;
            _configService = configService;
        }

        public async Task Consume(ConsumeContext<RunAllBotMessage> context)
        {
            _configService.DeleteAllConfig();
        }
    }
}
