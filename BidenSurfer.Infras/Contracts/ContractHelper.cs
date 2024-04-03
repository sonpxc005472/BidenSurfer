using OKX.Api.Enums;
using BidenSurfer.Infras.Models;

namespace BidenSurfer.Infras.Contracts
{
    public static class ContractHelper
    {
        public static OkxPeriod ToPeriod(this string candleStick)
        {
            return GetCandleSticks().FirstOrDefault(c => c.CandleStickName == candleStick).CandleStickPeriod;
        }

        public static string ToCandleStick(this OkxPeriod period)
        {
            return GetCandleSticks().FirstOrDefault(c => c.CandleStickPeriod == period).CandleStickName;
        }

        public static List<CandleStickMapping> GetCandleSticks()
        {
            return new List<CandleStickMapping>
            {
                new CandleStickMapping{ CandleStickName = "1m", CandleStickPeriod = OkxPeriod.OneMinute },
                new CandleStickMapping{ CandleStickName = "3m", CandleStickPeriod = OkxPeriod.ThreeMinutes },
                new CandleStickMapping{ CandleStickName = "5m", CandleStickPeriod = OkxPeriod.FiveMinutes },
                new CandleStickMapping{ CandleStickName = "15m", CandleStickPeriod = OkxPeriod.FifteenMinutes },
                new CandleStickMapping{ CandleStickName = "30m", CandleStickPeriod = OkxPeriod.ThirtyMinutes },
                new CandleStickMapping{ CandleStickName = "1H", CandleStickPeriod = OkxPeriod.OneHour },
                new CandleStickMapping{ CandleStickName = "2H", CandleStickPeriod = OkxPeriod.TwoHours },
                new CandleStickMapping{ CandleStickName = "4H", CandleStickPeriod = OkxPeriod.FourHours },
                new CandleStickMapping{ CandleStickName = "6H", CandleStickPeriod = OkxPeriod.SixHours },
                new CandleStickMapping{ CandleStickName = "12H", CandleStickPeriod = OkxPeriod.TwelveHours },
                new CandleStickMapping{ CandleStickName = "1D", CandleStickPeriod = OkxPeriod.OneDay }
            };
        }
    }
}
