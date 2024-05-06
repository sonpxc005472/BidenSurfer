using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
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
                _configService.AddOrEditConfigFromApi(configs);
                await _botService.SubscribeSticker();
            }                        
        }
    }
}
