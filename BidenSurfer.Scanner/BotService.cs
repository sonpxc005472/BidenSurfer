namespace BidenSurfer.Scanner.Services;

using BidenSurfer.Infras;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using MassTransit;
using System.Collections.Concurrent;
using Websocket.Client;
using System.Text.Json;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras.Models;

public interface IBotService
{
    Task<BybitSocketClient> SubscribeSticker();    
}

public class BotService : IBotService
{
    IBus _bus;
    IConfigService _configService;
    IScannerService _scannerService;
    public BotService(IBus bus, IConfigService configService, IScannerService scannerService)
    {
        _bus = bus;
        _configService = configService;
        _scannerService = scannerService;
    }

    public async Task<BybitSocketClient> SubscribeSticker()
    {
        await RunScanner();        
        return StaticObject.PublicWebsocket;
    }

    private static ConcurrentDictionary<string, Candle> _candles = new ConcurrentDictionary<string, Candle>();
    private static ConcurrentDictionary<string, long> _candle1s = new ConcurrentDictionary<string, long>();
    private static string _websocketUrl = "wss://stream.bybit.com/v5/public/spot"; // URL của websocket Bybit

    private async Task RunScanner()
    {
        var exitEvent = new ManualResetEvent(false);
        var url = new Uri(_websocketUrl);
        var symbols = StaticObject.Symbols.Where(s => (s.MarginTrading == MarginTrading.Both || s.MarginTrading == MarginTrading.UtaOnly) && s.Name.EndsWith("USDT")).Select(c => c.Name).Distinct().ToList();
        
        using (var client = new WebsocketClient(url))
        {
            client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ReconnectionHappened.Subscribe(info =>
                Console.WriteLine($"Reconnection happened, type: {info.Type}"));

            client.MessageReceived.Subscribe(async msg => {

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                if (!string.IsNullOrEmpty(msg.Text) && msg.Text.Contains("data"))
                {
                    var result = JsonSerializer.Deserialize<SubscribeObject>(msg.Text, options);
                    var data = result?.data;
                    foreach(var d in data)
                    {
                        var symbol = d.s;
                        var timestamp = d.T / 1000;
                        var tick = new TickData
                        {
                            Timestamp = d.T,
                            Price = decimal.Parse(d.p),
                            Amount = decimal.Parse(d.v) * decimal.Parse(d.p)
                        };
                        if (_candle1s.ContainsKey(symbol))
                        {
                            var preTimestamp = _candle1s[symbol];
                            if(timestamp == preTimestamp)
                            {
                                _candles.AddOrUpdate(symbol,
                                (ts) => new Candle // Tạo nến mới nếu chưa có
                                {
                                    Open = tick.Price,
                                    High = tick.Price,
                                    Low = tick.Price,
                                    Close = tick.Price,
                                    Volume = tick.Amount
                                },
                                (ts, existingCandle) => // Cập nhật nến hiện tại
                                {
                                    existingCandle.High = Math.Max(existingCandle.High, tick.Price);
                                    existingCandle.Low = Math.Min(existingCandle.Low, tick.Price);
                                    existingCandle.Close = tick.Price;
                                    existingCandle.Volume += tick.Amount;
                                    return existingCandle;
                                });
                            }
                            else if (timestamp > preTimestamp)
                            {
                                _candle1s[symbol] = timestamp;
                                var candle = _candles[symbol];
                                var longPercent = (candle.Low - candle.Open) / candle.Open * 100;
                                var shortPercent = (candle.High - candle.Open) / candle.Open * 100;
                                var longElastic = longPercent == 0 ? 0 : (longPercent - ((candle.Close - candle.Open) / candle.Open * 100)) / longPercent * 100;
                                var shortElastic = shortPercent == 0 ? 0 : (shortPercent - ((candle.Close - candle.Open) / candle.Open * 100)) / shortPercent * 100;
                                if (longPercent < (decimal)-0.8 && longElastic >= 60)
                                {
                                    Console.WriteLine($"{symbol}|long: {longPercent.ToString("0.00")}%|TP: {longElastic.ToString("0.00")}|Vol: {(candle.Volume / 1000).ToString("0.00")}K");
                                    Console.WriteLine($"{symbol}|open: {candle.Open.ToString()}|hight: {candle.High.ToString()}|low: {candle.Low.ToString()}|close: {candle.Close.ToString()}");
                                    Console.WriteLine($"=====================================================================================");
                                    var scanners = await _scannerService.GetAll();
                                    var configs = _configService.GetAllActive();
                                    bool isMatched = false;
                                    foreach (var scanner in scanners)
                                    {
                                        var scanOcExisted = configs.FirstOrDefault(c => c.Symbol == symbol && c.CreatedBy == AppConstants.CreatedByScanner && c.UserId == scanner.UserId && c.IsActive);
                                        if(scanOcExisted == null )
                                        {                                            
                                            //If scan indicator matched user's scanner configurations 
                                            if (scanner.PositionSide == AppConstants.LongSide && scanner.OrderChange <= -longPercent
                                                && scanner.Elastic <= longElastic && scanner.Turnover <= candle.Volume
                                                )
                                            {
                                                isMatched = true;
                                                // create new configs for long side
                                                var newConfigs = CalculateOcs(symbol, longPercent, scanner);
                                            }
                                        }                                           
                                    }
                                    if (isMatched)
                                    {
                                        await _bus.Send(new NewConfigCreatedMessage());
                                        await _bus.Send(new SaveNewConfigMessage());
                                    }                                       
                                }
                                if (shortPercent > (decimal)0.8 && shortElastic >= 60)
                                {
                                    Console.WriteLine($"{symbol}|short: {shortPercent.ToString("0.00")}%|TP: {shortElastic.ToString("0.00")}|Vol: {(candle.Volume / 1000).ToString("0.00")}K");
                                    Console.WriteLine($"{symbol}|open: {candle.Open.ToString()}|hight: {candle.High.ToString()}|low: {candle.Low.ToString()}|close: {candle.Close.ToString()}");
                                    Console.WriteLine($"=====================================================================================");
                                    var scanners = await _scannerService.GetAll();
                                    var configs = _configService.GetAllActive();
                                    bool isMatched = false;
                                    foreach (var scanner in scanners)
                                    {
                                        var scanOcExisted = configs.FirstOrDefault(c => c.IsActive && c.Symbol == symbol && c.CreatedBy == AppConstants.CreatedByScanner && c.UserId == scanner.UserId);
                                        if (scanOcExisted == null)
                                        {
                                            var instrument = StaticObject.Symbols.FirstOrDefault(x => x.Name == symbol);
                                            var isMarginTrading = instrument.MarginTrading == MarginTrading.Both;
                                            //If scan indicator matched user's scanner configurations 
                                            if (scanner.PositionSide == AppConstants.ShortSide && scanner.OrderChange <= shortPercent
                                                && scanner.Elastic <= shortElastic && scanner.Turnover <= candle.Volume && isMarginTrading
                                                )
                                            {
                                                isMatched = true;
                                                // create new configs for short side
                                                var newConfigs = CalculateOcs(symbol, shortPercent, scanner);
                                            }
                                        }
                                    }
                                    if (isMatched)
                                    {
                                        await _bus.Send(new NewConfigCreatedMessage());
                                        await _bus.Send(new SaveNewConfigMessage());
                                    }
                                }
                                _candles[symbol] =  new Candle
                                {
                                    Open = tick.Price,
                                    High = tick.Price,
                                    Low = tick.Price,
                                    Close = tick.Price,
                                    Volume = tick.Amount
                                };
                            }
                        }
                        else
                        {
                            _candle1s.TryAdd(symbol, timestamp);
                            _candles.TryAdd(symbol, new Candle
                            {
                                Open = tick.Price,
                                High = tick.Price,
                                Low = tick.Price,
                                Close = tick.Price,
                                Volume = tick.Amount
                            });
                        }
                        
                    }                     

                }

            });
            _ = client.Start();

            _ = Task.Run(() =>
            {

                foreach (var symbol in symbols)
                {
                    var messageSubs = $"{{\"op\":\"subscribe\",\"args\":[\"publicTrade.{symbol}\"]}}";
                    client.Send(messageSubs);
                }

            });

            exitEvent.WaitOne();
        }
    }
    
    private List<ConfigDto> CalculateOcs(string symbol, decimal maxOC, ScannerDto scanner)
    {
        var minOc = (Math.Abs(maxOC)-(decimal)0.2)/2;
        var rangeOc = minOc/scanner.OcNumber;
        var configs = new List<ConfigDto>();
        for(var i = 1; i <= scanner.OcNumber; i++)
        {
            var config = new ConfigDto
            {
                CustomId = Guid.NewGuid().ToString(),
                OrderChange = Math.Round(minOc + (rangeOc * i), 2),
                Amount = scanner.Amount,
                AmountLimit = scanner.AmountLimit,
                Expire = scanner.ConfigExpire,
                IncreaseAmountExpire = scanner.AmountExpire,
                IncreaseAmountPercent = scanner.AutoAmount,
                IncreaseOcPercent = 0,
                IsActive = true,
                OrderType = scanner.OrderType,
                Symbol = symbol,
                PositionSide = scanner.PositionSide,
                UserId = scanner.UserId,
                CreatedBy = AppConstants.CreatedByScanner,
                CreatedDate = DateTime.Now,
                isNewScan = true
            };
            configs.Add(config);
        }
        _configService.AddOrEditConfig(configs);
        return configs;
    }
}




public class SubscribeObject
{
    public SubscribeData[] data { get; set; }
}

public class SubscribeData
{
    public string i { get; set; }
    public long T { get; set; }
    public string p { get; set; }
    public string v { get; set; }
    public string s { get; set; }
}

public class TickData
{
    public long Timestamp { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
}

public class Candle
{
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
}