using BidenSurfer.Infras.BusEvents;
using BidenSurfer.WebApi.Services;
using MassTransit;

namespace BidenSurfer.WebApi.Consumers
{
    public class SaveNewConfigConsumer : IConsumer<SaveNewConfigMessage>
    {
        private readonly IConfigService _configService;
        public SaveNewConfigConsumer(IConfigService configService)
        {
            _configService = configService;
        }
        public async Task Consume(ConsumeContext<SaveNewConfigMessage> context)
        {
            await _configService.SaveNewScanToDb();   
        }
    }
}
