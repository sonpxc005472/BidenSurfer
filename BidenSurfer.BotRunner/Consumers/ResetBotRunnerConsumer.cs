using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras.BusEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BidenSurfer.BotRunner.Consumers
{
    public class ResetBotRunnerConsumer : IConsumer<ResetBotForBotRunnerMessage>
    {
        private readonly IBotService _botService;
        private readonly ILogger<ResetBotRunnerConsumer> _logger;

        public ResetBotRunnerConsumer(IBotService botService, ILogger<ResetBotRunnerConsumer> logger)
        {
            _botService = botService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ResetBotForBotRunnerMessage> context)
        {
            _logger.LogInformation("Resetting bot manually...");
            await _botService.ResetBot();
        }
    }
}
