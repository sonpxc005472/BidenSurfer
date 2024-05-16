using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.BotRunner.Consumers
{
    public class ConfigUpdateFromApiConsumer : IConsumer<ConfigUpdateFromApiForBotRunnerMessage>
    {
        private readonly IBotService _botService;
        private readonly IConfigService _configService;
        public ConfigUpdateFromApiConsumer(IBotService botService, IConfigService configService)
        {
            _botService = botService;
            _configService = configService;
        }
        public async Task Consume(ConsumeContext<ConfigUpdateFromApiForBotRunnerMessage> context)
        {
            var configs = context.Message?.ConfigDtos;
            if (configs != null && configs.Any())
            {
                Console.WriteLine($"ConfigUpdateFromApiConsumer: {string.Join(",", configs.Select(x => $"{x.Symbol} - Active:{x.IsActive} - OC: {x.OrderChange}").ToList())}");
                _configService.AddOrEditConfigFromApi(configs);
                await _botService.SubscribeSticker();
            }                        
        }
    }
}
