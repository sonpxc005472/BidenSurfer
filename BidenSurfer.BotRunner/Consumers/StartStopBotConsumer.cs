using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.BotRunner.Consumers
{
    public class StartStopBotConsumer : IConsumer<StartStopBotForBotRunnerMessage>
    {
        private readonly IBotService _botService;
        public StartStopBotConsumer(IBotService botService)
        {
            _botService = botService;
        }
        public async Task Consume(ConsumeContext<StartStopBotForBotRunnerMessage> context)
        {
            var userId = context.Message.UserId;
            var isStop = context.Message.IsStop;
            if (StaticObject.BotStatus.ContainsKey(userId))
            {
                StaticObject.BotStatus[userId] = !isStop;
            }
            else
            {
                StaticObject.BotStatus.Add(userId, !isStop);
            }           
            if (isStop)
                await _botService.CancelAllOrder(userId);
        }
    }
}
