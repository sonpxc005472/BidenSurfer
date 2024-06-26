using BidenSurfer.Infras.Models;
using System.Collections.Concurrent;
using Bybit.Net.Clients;
using Bybit.Net.Objects.Models.V5;
using CryptoExchange.Net.Objects.Sockets;

namespace BidenSurfer.Infras
{
    public static class StaticObject
    {
        public static ConcurrentDictionary<long, BybitRestClient> RestApis = new ConcurrentDictionary<long, BybitRestClient>();
        public static ConcurrentDictionary<long, BybitSocketClient> Sockets = new ConcurrentDictionary<long, BybitSocketClient>();       
        public static ConcurrentDictionary<string, decimal> ContractToCurrencies = new ConcurrentDictionary<string, decimal>();
        public static ConcurrentDictionary<string, UpdateSubscription> TickerSubscriptions = new ConcurrentDictionary<string, UpdateSubscription>();
        public static ConcurrentDictionary<string, UpdateSubscription> ScannerTradeSubscriptions = new ConcurrentDictionary<string, UpdateSubscription>();
        public static Dictionary<string, BybitKlineUpdate> Kline1mSubscriptions = new Dictionary<string, BybitKlineUpdate>();
        public static Dictionary<string, decimal> SymbolTurnover = new Dictionary<string, decimal>();
        public static Dictionary<long, bool> BotStatus = new Dictionary<long, bool>();
        public static Dictionary<long, bool> ScannerStatus = new Dictionary<long, bool>();
        public static ConcurrentDictionary<long, UpdateSubscription> OrderSubscriptions = new ConcurrentDictionary<long, UpdateSubscription>();
        public static ConcurrentDictionary<string,ConfigDto> FilledOrders = new ConcurrentDictionary<string,ConfigDto>();
        public static ConcurrentDictionary<string,ConfigDto> AllConfigs = new ConcurrentDictionary<string,ConfigDto>();
        public static ConcurrentDictionary<string,ConfigDto> TempCancelConfigs = new ConcurrentDictionary<string,ConfigDto>();
        public static List<ScannerDto> AllScanners = new List<ScannerDto>();
        public static List<ScannerSettingDto> AllScannerSetting = new List<ScannerSettingDto>();
        public static List<UserDto> AllUsers = new List<UserDto>();
        public static List<BybitSpotSymbol> Symbols = new List<BybitSpotSymbol>();
        public static bool IsInternalCancel = false;
    }

    public class BybitSocketClientSingleton
    {
        private static readonly Lazy<BybitSocketClient> instance = new(() => new BybitSocketClient());

        // Private constructor to prevent instantiation outside of the class
        private BybitSocketClientSingleton() { }

        public static BybitSocketClient Instance
        {
            get
            {
                return instance.Value;
            }
        }
    }
}
