using Bybit.Net.Enums;
using CryptoExchange.Net.CommonObjects;
using Telegram.Bot;

namespace BidenSurfer.Infras.Helpers
{
    public class TeleMessage : ITeleMessage
    {
        private readonly TelegramBotClient botClient;
        public TeleMessage() {
            botClient = new TelegramBotClient("6519727860:AAH34md0Aqu2RSavKU4kLDWnAzfXKiZSjSQ");
        }
        public async Task SendMessage(string message, string teleChannel)
        {
            try
            {   
                var text = $"{message}\n🔥 <b>BIDEN BYBIT V1</b>";

                await botClient.SendTextMessageAsync(teleChannel, text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        public async Task ScannerOpenMessage(string title, string symbol, string oc, string positionSide, string tele)
        {
            var text = $"❤ Scanner <b>{title.ToUpper()}</b> created {symbol} | {positionSide.ToUpper()} | {oc}";
            await SendMessage(text, tele);
        }
        
        public async Task OffConfigMessage(string symbol, string oc, string positionSide, string tele, string reason)
        {
            var text = $"&#128244; OFF <i>{symbol}</i> | {positionSide.ToUpper()} | {oc}\n<code>{reason}</code>";
            await SendMessage(text, tele);
        }

        public async Task FillMessage(string symbol, string oc, string positionSide, string tele, bool filled, decimal filledAmount, decimal orderAmount, decimal price)
        {
            var text = $"&#128680; {(filled ? "FILLED": "PARTIALLY FILLED")} <b>{symbol}</b> | {positionSide.ToUpper()} | {oc} \nPRICE: $<code>{price}</code> QUANTITY: <code>{filledAmount}/{orderAmount}</code>\nAMOUNT: $<code>{Math.Round(price * filledAmount,2)}</code>";
            await SendMessage(text, tele);
        }
        
        public async Task PnlMessage(string symbol, string oc, string positionSide, string tele, bool win, decimal pnlCash, decimal pnlPercent, int totalWin, int total)
        {
            var text = $"💰 <b>{symbol} | {positionSide.ToUpper()} | {(win? "WIN":"LOSE")} | {oc}</b>\n<b>PNL</b>: <code>${pnlCash} {pnlPercent}%</code>\nWIN RATE: <code>{totalWin}/{total}</code>";
            await SendMessage(text, tele);
        }
        
        public async Task ErrorMessage(string symbol, string oc, string positionSide, string tele, string error)
        {
            var text = $"🚩 <b>{symbol}</b> | {positionSide.ToUpper()} | {oc}\n<b>ERROR</b>: <code>{error}</code>";
            await SendMessage(text, tele);
        }

        public async Task WalletNotifyMessage(decimal balance, decimal budget, decimal pnlCash, decimal pnlPercent, string tele)
        {
            var text = $"💰 <b>BALANCE: ${balance}</b>\nBUDGET: <code>${budget}</code> | PNL: <code>${pnlCash}</code> ({pnlPercent}%)";
            await SendMessage(text, tele);
        }
    }
}
