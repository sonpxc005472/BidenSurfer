using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.Scanner.Consumers
{
    public class ConfigUpdateFromApiConsumer : IConsumer<ConfigUpdateFromApiForScannerMessage>
    {
        private readonly IConfigService _configService;
        public ConfigUpdateFromApiConsumer(IConfigService configService)
        {
           _configService = configService;
        }
        public async Task Consume(ConsumeContext<ConfigUpdateFromApiForScannerMessage> context)
        {
            var configs = context.Message?.ConfigDtos;
            if (configs != null && configs.Any())
            {
                _configService.AddOrEditConfig(configs);
            }                        
        }
    }
}
