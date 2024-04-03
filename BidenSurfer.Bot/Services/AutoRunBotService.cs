using MassTransit;
using BidenSurfer.Infras.BusEvents;

namespace BidenSurfer.Bot.Services
{
    public class AutoRunBotService : BackgroundService
    {
        private readonly IBus _bus;
        public AutoRunBotService(IBus bus)
        {
            _bus = bus;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _bus.Send(new RunAllBotMessage { CorrelationId = Guid.NewGuid() }, cancellationToken);
        }
    }
}
