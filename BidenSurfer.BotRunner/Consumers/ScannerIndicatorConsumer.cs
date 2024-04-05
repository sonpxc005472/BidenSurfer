using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras.BusEvents;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BidenSurfer.BotRunner.Consumers
{
    public class ScannerIndicatorConsumer : IConsumer<NewConfigCreatedMessage>
    {
        private readonly IBotService _botService;
        public ScannerIndicatorConsumer(IBotService botService)
        {
            _botService = botService;
        }
        public async Task Consume(ConsumeContext<NewConfigCreatedMessage> context)
        {
            await _botService.SubscribeSticker();
        }
    }
}
