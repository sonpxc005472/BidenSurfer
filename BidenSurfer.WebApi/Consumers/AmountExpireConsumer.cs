using BidenSurfer.Infras.BusEvents;
using BidenSurfer.WebApi.Services;
using MassTransit;

namespace BidenSurfer.WebApi.Consumers
{
    public class AmountExpireConsumer : IConsumer<AmountExpireMessage>
    {
        private readonly IConfigService _configService;
        public AmountExpireConsumer(IConfigService configService)
        {
            _configService = configService;
        }
        public async Task Consume(ConsumeContext<AmountExpireMessage> context)
        {
            var customeIds = context.Message.Configs;
            await _configService.AmountExpireUpdate(customeIds);   
        }
    }
}
