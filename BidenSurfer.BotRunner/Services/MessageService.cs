using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace BidenSurfer.BotRunner.Services
{
    public interface IMessageService
    {
        void ReceiveMessage();
    }
    public class MessageService : IMessageService
    {
        private ConnectionFactory factory;
        private IBotService _botService;
        public MessageService(IBotService botService) {
            factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            _botService = botService;
        }
        // Dịch vụ 2: Nhận tin nhắn
        public void ReceiveMessage()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    await _botService.SubscribeSticker();
                };
                channel.BasicConsume(queue: "hello",
                                     autoAck: true,
                                     consumer: consumer);
                
            }
        }
    }
}
