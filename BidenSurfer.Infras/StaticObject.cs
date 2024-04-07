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
        public static BybitSocketClient PublicWebsocket = new BybitSocketClient();
        public static ConcurrentDictionary<string, decimal> ContractToCurrencies = new ConcurrentDictionary<string, decimal>();
        public static ConcurrentDictionary<string, UpdateSubscription> TickerSubscriptions = new ConcurrentDictionary<string, UpdateSubscription>();
        public static Dictionary<string, BybitKlineUpdate> Kline1mSubscriptions = new Dictionary<string, BybitKlineUpdate>();
        public static Dictionary<string, decimal> SymbolTurnover = new Dictionary<string, decimal>();
        public static ConcurrentDictionary<long, UpdateSubscription> OrderSubscriptions = new ConcurrentDictionary<long, UpdateSubscription>();
        public static ConcurrentBag<ConfigDto> FilledOrders = new ConcurrentBag<ConfigDto>();
        public static List<ConfigDto> AllConfigs = new List<ConfigDto>();
        public static List<BybitSpotSymbol> Symbols = new List<BybitSpotSymbol>();
    }
}
