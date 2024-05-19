using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.Scanner.Consumers
{
    public class StartStopBotConsumer : IConsumer<StartStopBotForScannerMessage>
    {
        public StartStopBotConsumer()
        {
        }
        public async Task Consume(ConsumeContext<StartStopBotForScannerMessage> context)
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
        }
    }
}
