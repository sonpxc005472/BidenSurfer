using BidenSurfer.Infras.BusEvents;
using BidenSurfer.WebApi.Services;
using MassTransit;

namespace BidenSurfer.WebApi.Consumers
{
    public class UpdateConfigConsumer : IConsumer<UpdateConfigMessage>
    {
        private readonly IConfigService _configService;
        public UpdateConfigConsumer(IConfigService configService)
        {
            _configService = configService;
        }
        public async Task Consume(ConsumeContext<UpdateConfigMessage> context)
        {
            var configDtos = context.Message.Configs;
            foreach(var config in configDtos)
            {
                await _configService.AddOrEdit(config);
            }
        }
    }
}
