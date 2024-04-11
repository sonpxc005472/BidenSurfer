using Telegram.Bot;

namespace BidenSurfer.Infras.Helpers
{
    public static class TelegramHelper
    {
        public static async Task SendMessage(string message, string teleChannel)
        {
            var botClient = new TelegramBotClient("6519727860:AAH34md0Aqu2RSavKU4kLDWnAzfXKiZSjSQ");
            var text = $"{message}\n&#9757; <b>Bybit V1</b>";

            await botClient.SendTextMessageAsync(teleChannel, text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }

        public static async Task ScannerOpenMessage(string title, string symbol, string oc, string positionSide, string tele)
        {
            var text = $"&#9203; Scanner <b><i>{title}</i></b> created\n{symbol} | {positionSide.ToUpper()}\nOC: {oc}";
            await SendMessage(text, tele);
        }
        
        public static async Task OffConfigMessage(string symbol, string oc, string positionSide, string tele, string reason)
        {
            var text = $"&#128244; Off <i>{symbol}</i> | {positionSide.ToUpper()} | {oc}\n{reason}";
            await SendMessage(text, tele);
        }

        public static async Task FillMessage(string symbol, string oc, string positionSide, string tele, bool filled, decimal filledAmount, decimal orderAmount)
        {
            var text = $"&#128680; {(filled ? "FILLED": "PARTIALLY FILLED")} <i>{symbol}</i> | {positionSide.ToUpper()} | {oc} \nAMOUNT: {filledAmount}/{orderAmount}";
            await SendMessage(text, tele);
        }
        
        public static async Task PnlMessage(string symbol, string oc, string positionSide, string tele, bool win, decimal pnlCash, decimal pnlPercent)
        {
            var text = $"<b>{symbol}<b> | {positionSide.ToUpper()} | {(win? "WIN":"LOSE")} \nOC: {oc}\n PNL: ${pnlCash} {pnlPercent}%";
            await SendMessage(text, tele);
        }
    }
}
