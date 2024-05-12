using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BidenSurfer.Infras.Helpers
{
    public interface ITeleMessage
    {
        Task SendMessage(string message, string teleChannel);
        Task ScannerOpenMessage(string title, string symbol, string oc, string positionSide, string tele);
        Task OffConfigMessage(string symbol, string oc, string positionSide, string tele, string reason);
        Task FillMessage(string symbol, string oc, string positionSide, string tele, bool filled, decimal filledAmount, decimal orderAmount, decimal price);
        Task PnlMessage(string symbol, string oc, string positionSide, string tele, bool win, decimal pnlCash, decimal pnlPercent, int totalWin, int total);
        Task WalletNotifyMessage(decimal balance, decimal budget, decimal pnlCash, decimal pnlPercent, string tele);
        Task ErrorMessage(string symbol, string oc, string positionSide, string tele, string error);
    }
}
