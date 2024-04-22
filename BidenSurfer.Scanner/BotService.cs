﻿namespace BidenSurfer.Scanner.Services;

using BidenSurfer.Infras;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using MassTransit;
using System.Collections.Concurrent;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.BotRunner.Services;
using BidenSurfer.Infras.Models;
using System.Collections.Generic;
using BidenSurfer.Infras.Helpers;
using BidenSurfer.Infras.Entities;

public interface IBotService
{
    Task<BybitSocketClient> SubscribeSticker();
}

public class BotService : IBotService
{
    IBus _bus;
    IConfigService _configService;
    IScannerService _scannerService;
    private readonly ITeleMessage _teleMessage;
    public BotService(IBus bus, IConfigService configService, IScannerService scannerService, ITeleMessage teleMessage)
    {
        _bus = bus;
        _configService = configService;
        _scannerService = scannerService;
        _teleMessage = teleMessage;
    }

    public async Task<BybitSocketClient> SubscribeSticker()
    {
        await RunScanner();
        return StaticObject.PublicWebsocket;
    }

    private static ConcurrentDictionary<string, Candle> _candles = new ConcurrentDictionary<string, Candle>();
    private static ConcurrentDictionary<string, long> _candle1s = new ConcurrentDictionary<string, long>();

    private async Task RunScanner()
    {
        //var exitEvent = new ManualResetEvent(false);
        //var url = new Uri(_websocketUrl);
        var totalsymbols = StaticObject.Symbols.Where(s => (s.MarginTrading == MarginTrading.Both || s.MarginTrading == MarginTrading.UtaOnly) && s.Name.EndsWith("USDT")).Select(c => c.Name).Distinct().ToList();
        var batches = totalsymbols.Select((x, i) => new { Index = i, Value = x })
                          .GroupBy(x => x.Index / 10)
                          .Select(x => x.Select(v => v.Value).ToList())
                          .ToList();
        foreach (var symbols in batches)
        {
            var subResult = await StaticObject.PublicWebsocket.V5SpotApi.SubscribeToTradeUpdatesAsync(symbols, async data =>
            {
                if (data != null)
                {
                    var tradeDatas = data.Data;
                    foreach (var tradeData in tradeDatas)
                    {
                        var symbol = tradeData.Symbol;
                        long converttimestamp = (long)(tradeData.Timestamp.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        var timestamp = converttimestamp / 1000;
                        var tick = new TickData
                        {
                            Timestamp = converttimestamp,
                            Price = tradeData.Price,
                            Amount = tradeData.Price * tradeData.Quantity
                        };
                        if (_candle1s.ContainsKey(symbol))
                        {
                            var preTimestamp = _candle1s[symbol];
                            if (timestamp == preTimestamp)
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
                                var candle = _candles[symbol];
                                if (!candle.Confirmed)
                                {
                                    var longPercent = (candle.Low - candle.Open) / candle.Open * 100;
                                    var shortPercent = (candle.High - candle.Open) / candle.Open * 100;
                                    var longElastic = longPercent == 0 ? 0 : (longPercent - ((candle.Close - candle.Open) / candle.Open * 100)) / longPercent * 100;
                                    var shortElastic = shortPercent == 0 ? 0 : (shortPercent - ((candle.Close - candle.Open) / candle.Open * 100)) / shortPercent * 100;
                                    if (longPercent < (decimal)-1 && longElastic >= 70)
                                    {
                                        var scanners = StaticObject.AllScanners;
                                        var configs = StaticObject.AllConfigs;
                                        bool isMatched = false;
                                        var newConfigs = new List<ConfigDto>();
                                        foreach (var scanner in scanners)
                                        {
                                            var scanOcExisted = configs.Any(c => c.Value.Symbol == symbol && c.Value.CreatedBy == AppConstants.CreatedByScanner && c.Value.UserId == scanner.UserId && c.Value.IsActive);
                                            if (!scanOcExisted)
                                            {
                                                var scannerSetting = StaticObject.AllScannerSetting.FirstOrDefault(r => r.UserId == scanner.UserId);
                                                var blackList = scannerSetting?.BlackList ?? new List<string>();
                                                var maxOpen = scannerSetting?.MaxOpen ?? 4; // We can only open 4 orders by default
                                                var numScannerOpen = configs.Count(c=> c.Value.UserId == scanner.UserId && c.Value.CreatedBy == AppConstants.CreatedByScanner && c.Value.IsActive && !string.IsNullOrEmpty(c.Value.OrderId));
                                                var symbolDetail = StaticObject.Symbols.FirstOrDefault(x => x.Name == symbol);
                                                //If scan indicator matched user's scanner configurations 
                                                if (scanner.PositionSide == AppConstants.LongSide && scanner.OrderChange <= -longPercent
                                                    && scanner.Elastic <= longElastic && scanner.Turnover <= candle.Volume && !blackList.Any(b => b == symbolDetail?.BaseAsset)
                                                    )
                                                {
                                                    isMatched = true;
                                                    candle.Confirmed = true;
                                                    Console.WriteLine($"{symbol}|long: {longPercent.ToString("0.00")}%|TP: {longElastic.ToString("0.00")}|Vol: {(candle.Volume / 1000).ToString("0.00")}K");
                                                    Console.WriteLine($"{symbol}|open: {candle.Open.ToString()}|hight: {candle.High.ToString()}|low: {candle.Low.ToString()}|close: {candle.Close.ToString()}");
                                                    Console.WriteLine($"Matched: {DateTime.Now.ToString("dd/MM/yy HH:mm:ss.fff")}");
                                                    Console.WriteLine($"=====================================================================================");

                                                    // create new configs for long side
                                                    newConfigs = await CalculateOcs(symbol, longPercent, scanner, maxOpen, numScannerOpen);
                                                }
                                            }
                                        }
                                        if (isMatched && newConfigs.Any())
                                        {
                                            _configService.AddOrEditConfig(newConfigs);
                                            await _bus.Send(new NewConfigCreatedMessage());
                                            await _bus.Send(new SaveNewConfigMessage());
                                            foreach (var config in newConfigs)
                                            {
                                                var userSetting = StaticObject.AllUsers.FirstOrDefault(x => x.Id == config.UserId)?.Setting;
                                                await _teleMessage.ScannerOpenMessage(config.ScannerTitle, symbol, config.OrderChange.ToString(), config.PositionSide, userSetting.TeleChannel);
                                            }
                                        }
                                    }
                                    if (shortPercent > (decimal)1 && shortElastic >= 70)
                                    {
                                        var scanners = StaticObject.AllScanners;
                                        var configs = StaticObject.AllConfigs;
                                        bool isMatched = false;
                                        var newConfigs = new List<ConfigDto>();
                                        foreach (var scanner in scanners)
                                        {
                                            var scanOcExisted = configs.Any(c => c.Value.IsActive && c.Value.Symbol == symbol && c.Value.CreatedBy == AppConstants.CreatedByScanner && c.Value.UserId == scanner.UserId);
                                            if (!scanOcExisted)
                                            {
                                                var scannerSetting = StaticObject.AllScannerSetting.FirstOrDefault(r => r.UserId == scanner.UserId);
                                                var blackList = scannerSetting?.BlackList ?? new List<string>();
                                                var maxOpen = scannerSetting?.MaxOpen ?? 4; // We can only open 4 orders by default
                                                var numScannerOpen = configs.Count(c => c.Value.UserId == scanner.UserId && c.Value.CreatedBy == AppConstants.CreatedByScanner && c.Value.IsActive && !string.IsNullOrEmpty(c.Value.OrderId));
                                                var instrument = StaticObject.Symbols.FirstOrDefault(x => x.Name == symbol);
                                                var isMarginTrading = (instrument?.MarginTrading == MarginTrading.Both || instrument?.MarginTrading == MarginTrading.UtaOnly);
                                                //If scan indicator matched user's scanner configurations 
                                                if (scanner.PositionSide == AppConstants.ShortSide && scanner.OrderChange <= shortPercent
                                                    && scanner.Elastic <= shortElastic && scanner.Turnover <= candle.Volume && isMarginTrading && !blackList.Any(b => b == instrument?.BaseAsset)
                                                    )
                                                {
                                                    isMatched = true;
                                                    candle.Confirmed = true;
                                                    Console.WriteLine($"{symbol}|short: {shortPercent.ToString("0.00")}%|TP: {shortElastic.ToString("0.00")}|Vol: {(candle.Volume / 1000).ToString("0.00")}K");
                                                    Console.WriteLine($"{symbol}|open: {candle.Open.ToString()}|hight: {candle.High.ToString()}|low: {candle.Low.ToString()}|close: {candle.Close.ToString()}");
                                                    Console.WriteLine($"Matched: {DateTime.Now.ToString("dd/MM/yy HH:mm:ss.fff")}");
                                                    Console.WriteLine($"=====================================================================================");

                                                    // create new configs for short side
                                                    newConfigs = await CalculateOcs(symbol, shortPercent, scanner, maxOpen, numScannerOpen);
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine($"Scan OC existed: {symbol}");
                                            }
                                        }
                                        if (isMatched && newConfigs.Any())
                                        {
                                            _configService.AddOrEditConfig(newConfigs);
                                            await _bus.Send(new NewConfigCreatedMessage());
                                            await _bus.Send(new SaveNewConfigMessage());
                                            foreach (var config in newConfigs)
                                            {
                                                var userSetting = StaticObject.AllUsers.FirstOrDefault(x => x.Id == config.UserId)?.Setting;
                                                await _teleMessage.ScannerOpenMessage(config.ScannerTitle, symbol, config.OrderChange.ToString(), config.PositionSide, userSetting.TeleChannel);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (timestamp > preTimestamp)
                            {
                                _candle1s[symbol] = timestamp;

                                _candles[symbol] = new Candle
                                {
                                    Open = tick.Price,
                                    High = tick.Price,
                                    Low = tick.Price,
                                    Close = tick.Price,
                                    Volume = tick.Amount,
                                    Confirmed = false
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
                                Volume = tick.Amount,
                                Confirmed = false
                            });
                        }
                    }
                }
            });

            if (!subResult.Success)
            {
                Console.WriteLine("subscribe trade error: " + subResult.Error);
            }
        }
        
    }

    private async Task<List<ConfigDto>> CalculateOcs(string symbol, decimal maxOC, ScannerDto scanner, int maxOpen, int currentOpen)
    {
        var userSetting = StaticObject.AllUsers.FirstOrDefault(x => x.Id == scanner.UserId)?.Setting;
        var maxOcAbs = Math.Abs(maxOC);
        var minOc = (maxOcAbs - (decimal)0.2) / (maxOcAbs > 4 ? 3 : 2);
        var rangeOc = minOc / scanner.OcNumber;
        var configs = new List<ConfigDto>();
        for (var i = 1; i <= scanner.OcNumber; i++)
        {
            var oc = Math.Round(minOc + (rangeOc * i), 2);

            if (currentOpen + i > maxOpen)
            {
                await _teleMessage.ErrorMessage(symbol, oc.ToString(), scanner.PositionSide, userSetting.TeleChannel, $"Exceeded open limt {maxOpen}");
                continue;
            }
            currentOpen++;
            var config = new ConfigDto
            {
                CustomId = Guid.NewGuid().ToString(),
                OrderChange = oc,
                Amount = scanner.Amount,
                OriginAmount = scanner.Amount,
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
                EditedDate = DateTime.Now,
                isNewScan = true,
                ScannerTitle = scanner.Title
            };
            configs.Add(config);            
        }
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
    public bool Confirmed { get; set; }
}