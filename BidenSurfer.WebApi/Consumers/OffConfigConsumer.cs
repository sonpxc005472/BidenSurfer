using BidenSurfer.Infras.BusEvents;
using BidenSurfer.WebApi.Services;
using MassTransit;

namespace BidenSurfer.WebApi.Consumers
{
    public class OffConfigConsumer : IConsumer<OffConfigMessage>
    {
        private readonly IConfigService _configService;
        public OffConfigConsumer(IConfigService configService)
        {
            _configService = configService;
        }
        public async Task Consume(ConsumeContext<OffConfigMessage> context)
        {
            var customeIds = context.Message.CustomIds;
            await _configService.OffConfigs(customeIds);   
        }
    }
}
