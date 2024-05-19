using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using MassTransit;

namespace BidenSurfer.Scanner.Consumers
{
    public class StartStopScannerConsumer : IConsumer<StartStopScannerMessage>
    {
        public StartStopScannerConsumer()
        {
        }
        public async Task Consume(ConsumeContext<StartStopScannerMessage> context)
        {
            var userId = context.Message.UserId;
            var isStop = context.Message.IsStop;
            if (StaticObject.ScannerStatus.ContainsKey(userId))
            {
                StaticObject.ScannerStatus[userId] = !isStop;
            }
            else
            {
                StaticObject.ScannerStatus.Add(userId, !isStop);
            }                       
        }
    }
}
