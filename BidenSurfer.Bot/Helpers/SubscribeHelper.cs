using BidenSurfer.Infras;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BidenSurfer.Bot.Helpers
{
    public static class SubscribeHelper
    {
        public async static Task SendDataAsync(this ClientWebSocket ws, DataSend data)
        {
            string jsonString = JsonSerializer.Serialize(data);
            var encoded = Encoding.UTF8.GetBytes(jsonString);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
