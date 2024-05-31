using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Helpers;
using MassTransit;

namespace BidenSurfer.WebApi.Consumers
{
    public class SendTeleMessageConsumer : IConsumer<SendTeleMessage>
    {
        private readonly ILogger<SendTeleMessageConsumer> _logger;
        private readonly ITeleMessage _tele;
        public SendTeleMessageConsumer(ILogger<SendTeleMessageConsumer> logger, ITeleMessage tele)
        {
            _logger = logger;
            _tele = tele;
        }
        public async Task Consume(ConsumeContext<SendTeleMessage> context)
        {
            var message = context.Message.Message;
            var teleChannel = context.Message.TeleChannel;
            _logger.LogInformation($"SendTeleMessageConsumer: message: {message} - {teleChannel}");
            await _tele.SendMessage(message, teleChannel);
        }
    }
}
