using BidenSurfer.Infras.BusEvents;
using BidenSurfer.WebApi.Services;
using MassTransit;

namespace BidenSurfer.WebApi.Consumers
{
    public class AmountExpireConsumer : IConsumer<AmountExpireMessage>
    {
        private readonly IConfigService _configService;
        private readonly ILogger<AmountExpireConsumer> _logger;
        public AmountExpireConsumer(IConfigService configService, ILogger<AmountExpireConsumer> logger)
        {
            _configService = configService;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<AmountExpireMessage> context)
        {
            var customeIds = context.Message.Configs;
            _logger.LogInformation($"AmountExpireConsumer - {string.Join(",", customeIds)}");
            await _configService.AmountExpireUpdate(customeIds);   
        }
    }
}
