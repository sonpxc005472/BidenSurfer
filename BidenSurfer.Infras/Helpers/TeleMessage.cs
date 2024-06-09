using BidenSurfer.Infras.BusEvents;
using CryptoExchange.Net.CommonObjects;
using MassTransit;
using Telegram.Bot;

namespace BidenSurfer.Infras.Helpers
{
    public class TeleMessage : ITeleMessage
    {
        private readonly TelegramBotClient botClient;
        private readonly IBus _bus;
        public TeleMessage(IBus bus) {
            botClient = new TelegramBotClient("6519727860:AAH34md0Aqu2RSavKU4kLDWnAzfXKiZSjSQ");
            _bus = bus;
        }
        public async Task SendMessage(string message, string teleChannel)
        {
            var text = $"{message}\n🔥 <b>BIDEN BYBIT V1</b>";

            var sentMessage = await botClient.SendTextMessageAsync(teleChannel, text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            if (sentMessage.MessageId <= 0)
            {
                throw new Exception("Telegram message not sent");
            }
        }

        public async Task ScannerOpenMessage(string title, string symbol, string oc, string positionSide, string tele)
        {
            var text = $"❤ Scanner <b>{title.ToUpper()}</b> created {symbol} | {positionSide.ToUpper()} | {oc}";
            await _bus.Send(new SendTeleMessage { Message = text, TeleChannel = tele });
        }
        
        public async Task OffConfigMessage(string symbol, string oc, string positionSide, string tele, string reason)
        {
            var text = $"&#128244; OFF <i>{symbol}</i> | {positionSide.ToUpper()} | {oc}\n<code>{reason}</code>";
            await _bus.Send(new SendTeleMessage { Message = text, TeleChannel = tele });
        }

        public async Task FillMessage(string symbol, string oc, string positionSide, string tele, bool filled, decimal filledAmount, decimal orderAmount, decimal price)
        {
            var text = $"&#128680; {(filled ? "FILLED": "PARTIALLY FILLED")} <b>{symbol}</b> | {positionSide.ToUpper()} | {oc} \nPRICE: $<code>{price}</code> QUANTITY: <code>{filledAmount}/{orderAmount}</code>\nAMOUNT: $<code>{Math.Round(price * filledAmount,2)}</code>";
            await _bus.Send(new SendTeleMessage { Message = text, TeleChannel = tele });
        }

        public async Task PnlMessage(string symbol, string oc, string positionSide, string tele, bool win, decimal pnlCash, decimal pnlPercent, int totalWin, int total, decimal filledAmount, decimal orderAmount, decimal openPrice, decimal closePrice)
        {
            var text = $"💰 <b>{symbol} | {positionSide.ToUpper()} | {(win? "WIN":"LOSE")} | {oc}</b>\nOPEN: <code>${openPrice}</code>, CLOSE: <code>${closePrice}</code>\nQUANTITY: <code>{filledAmount}/{orderAmount}</code>\n <b>PNL</b>: <code>${pnlCash} {pnlPercent}%</code>\nWIN RATE: <code>{totalWin}/{total}</code>";
            await _bus.Send(new SendTeleMessage { Message = text, TeleChannel = tele });
        }

        public async Task ErrorMessage(string symbol, string oc, string positionSide, string tele, string error)
        {
            var text = $"🚩 <b>{symbol}</b> | {positionSide.ToUpper()} | {oc}\n<b>ERROR</b>: <code>{error}</code>";
            await _bus.Send(new SendTeleMessage { Message = text, TeleChannel = tele });
        }

        public async Task WalletNotifyMessage(decimal balance, decimal budget, decimal pnlCash, decimal pnlPercent, string tele)
        {
            var text = $"💰 <b>BALANCE: ${balance}</b>\nBUDGET: <code>${budget}</code> | PNL: <code>${pnlCash}</code> ({pnlPercent}%)";
            await _bus.Send(new SendTeleMessage { Message = text, TeleChannel = tele });
        }

        public async Task AssetTrackingMessage(string tele, string message)
        {
            var text = $"<b>WARNING!!! - ASSET TRACKING</b>\n{message}";
            await _bus.Send(new SendTeleMessage { Message = text, TeleChannel = tele });
        }
    }
}
