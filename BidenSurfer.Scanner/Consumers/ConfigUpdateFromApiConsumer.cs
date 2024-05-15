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
                Console.WriteLine($"ConfigUpdateFromApiConsumer: {string.Join(",", configs.Select(x => $"{x.CustomId} - Active:{x.IsActive} - OC: {x.OrderChange}").ToList())}");
                _configService.AddOrEditConfig(configs);
            }                        
        }
    }
}
