using RabbitMQ.Client;
using System.Text;

namespace BidenSurfer.Scanner
{
    public interface ISendMessageHelper
    {
        void SendMessage(string message);
    }

    public class SendMessageHelper
    {
        private ConnectionFactory factory;
        public SendMessageHelper()
        {
            factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password="guest" };
        }
        // Dịch vụ 1: Gửi tin nhắn
        public void SendMessage(string message)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body);
            }
        }               

    }
}
