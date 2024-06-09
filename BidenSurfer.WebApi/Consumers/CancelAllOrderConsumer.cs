using BidenSurfer.Infras.BusEvents;
using BidenSurfer.WebApi.Services;
using MassTransit;

namespace BidenSurfer.WebApi.Consumers
{
    public class CancelAllOrderConsumer : IConsumer<CancelAllOrderForApiMessage>
    {
        private readonly IConfigService _configService;
        public CancelAllOrderConsumer(IConfigService configService)
        {
            _configService = configService;
        }

        public async Task Consume(ConsumeContext<CancelAllOrderForApiMessage> context)
        {            
            await _configService.OffAllConfigs();   
        }
    }
}
