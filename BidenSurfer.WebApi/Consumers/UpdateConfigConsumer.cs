using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Models;
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
            Console.WriteLine($"UpdateConfigConsumer: {string.Join(",", configDtos.Select(x => $"{x.Symbol} - Active:{x.IsActive} - OC: {x.OrderChange}").ToList())}");
            foreach(var config in configDtos)
            {
                var configUpdate = new AddEditConfigDto
                {
                    Id = config.Id,
                    Amount = config.Amount,
                    AmountLimit = config.AmountLimit,
                    Expire = config.Expire,
                    IncreaseAmountExpire = config.IncreaseAmountExpire,
                    IncreaseAmountPercent = config.IncreaseAmountPercent,
                    IncreaseOcPercent = config.IncreaseOcPercent,
                    IsActive = config.IsActive,
                    OrderChange = config.OrderChange,
                    OrderType = config.OrderType,
                    PositionSide = config.PositionSide,
                    Symbol = config.Symbol
                };
                await _configService.AddOrEdit(configUpdate, true);
            }
        }
    }
}
