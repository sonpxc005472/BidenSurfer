﻿namespace BidenSurfer.Scanner.Services;

using BidenSurfer.Infras;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using MassTransit;
using System.Collections.Concurrent;
using Websocket.Client;
using System.Text.Json;

public interface IBotService
{
    Task<BybitSocketClient> SubscribeSticker();    
}

public class BotService : IBotService
{
    IBus _bus;
    public BotService(IBus bus)
    {
        _bus = bus;
    }

    public async Task<BybitSocketClient> SubscribeSticker()
    {
        RunScanner();
        //var symbols = StaticObject.Symbols.Where(s => s.MarginTrading == MarginTrading.Both && s.Name.EndsWith("USDT")).Select(c => c.Name).Distinct().ToList();

        //foreach (var symbol in symbols)
        //{
        //    decimal? openPrice = null;
        //    decimal? closePrice = null;
        //    decimal? hightPrice = null;
        //    decimal? lowPrice = null;
        //    decimal turnOver = 0;
            //await StaticObject.PublicWebsocket.V5SpotApi.SubscribeToTickerUpdatesAsync(symbol, async data =>
            //{
            //    if (data != null)
            //    {
            //        var currentData = data.Data;
            //        var currentTime = DateTime.Now;
            //        var currentPrice = currentData.LastPrice;

            //        if ((currentTime - preTimeTicker).TotalMilliseconds <= 1000)
            //        {
            //            if(openPrice == null)
            //            {
            //                openPrice = currentPrice;
            //                hightPrice = currentPrice;
            //                lowPrice = currentPrice;
            //            }
            //            else
            //            {
            //                if(currentPrice > hightPrice)
            //                {
            //                    hightPrice = currentPrice;
            //                }
            //                if(currentPrice < lowPrice)
            //                {
            //                    lowPrice = currentPrice;
            //                }
            //            }                        

            //        }
            //        else
            //        {
            //            closePrice = currentPrice;
            //            var currentTurnOver = (turnOver/1000);                        
            //            var longPercent = (lowPrice - openPrice) / openPrice * 100;   
            //            var shortPercent = (hightPrice - openPrice) / openPrice * 100;
            //            var longElastic = longPercent == 0 ? 0 : (longPercent - ((closePrice - openPrice) / openPrice * 100))/longPercent;
            //            var shortElastic = shortPercent == 0 ? 0 : (shortPercent - ((closePrice - openPrice) / openPrice * 100)) / shortPercent;
            //            if(longPercent < (decimal)-0.5)
            //            {
            //                Console.WriteLine($"{symbol}|long: {longPercent?.ToString("0.00")}%|TP: {longElastic?.ToString("0.00")}|Vol: {currentTurnOver.ToString("0.00")}K");
            //            }
            //            if (shortPercent > (decimal)0.5)
            //            {
            //                Console.WriteLine($"{symbol}|short: {shortPercent?.ToString("0.00")}%|TP: {shortElastic?.ToString("0.00")}|Vol: {currentTurnOver.ToString("0.00")}K");
            //            }
            //            turnOver = 0;
            //            preTimeTicker = currentTime;
            //            openPrice = null;
            //            hightPrice = null;
            //            lowPrice = null;
            //        }

            //    }

            //});

            //int preSecond = -1;
            //int preMilisecond = -1;

            //await StaticObject.PublicWebsocket.V5SpotApi.SubscribeToTradeUpdatesAsync(new List<string> { symbol }, async data =>
            //{
            //    if (data?.Data != null)
            //    {
            //        var currentData = data.Data.FirstOrDefault();
            //        var currentTime = DateTime.Now;
            //        var timestamp = currentData.Timestamp;
            //        var second = timestamp.Second;
            //        var milisecond = timestamp.Millisecond;
            //        var tradingVol = currentData.Quantity;
            //        var tradingPrice = currentData.Price;
            //        var tradingTurnover = tradingPrice * tradingVol;
            //        if (preSecond == second)
            //        {
            //            turnOver += tradingTurnover;

            //            if (currentData.Price > hightPrice)
            //            {
            //                hightPrice = tradingPrice;
            //            }
            //            if (currentData.Price < lowPrice)
            //            {
            //                lowPrice = tradingPrice;
            //            }
            //            closePrice = tradingPrice;
            //            preMilisecond = milisecond;
            //        }
            //        else
            //        {
            //            if (openPrice != null)
            //            {
            //                var longPercent = (lowPrice - openPrice) / openPrice * 100;
            //                var shortPercent = (hightPrice - openPrice) / openPrice * 100;
            //                var longElastic = longPercent == 0 ? 0 : (longPercent - ((closePrice - openPrice) / openPrice * 100)) / longPercent * 100;
            //                var shortElastic = shortPercent == 0 ? 0 : (shortPercent - ((closePrice - openPrice) / openPrice * 100)) / shortPercent * 100;
            //                if (longPercent < (decimal)-0.5)
            //                {
            //                    Console.WriteLine($"{symbol}|long: {longPercent?.ToString("0.00")}%|TP: {longElastic?.ToString("0.00")}|Vol: {(turnOver / 1000).ToString("0.00")}K");
            //                    Console.WriteLine($"{symbol}|open: {openPrice?.ToString()}|hight: {hightPrice?.ToString()}|low: {lowPrice.ToString()}|close: {closePrice.ToString()}");
            //                    Console.WriteLine($"=====================================================================================");
            //                }
            //                if (shortPercent > (decimal)0.5)
            //                {
            //                    Console.WriteLine($"{symbol}|short: {shortPercent?.ToString("0.00")}%|TP: {shortElastic?.ToString("0.00")}|Vol: {(turnOver / 1000).ToString("0.00")}K");
            //                    Console.WriteLine($"{symbol}|open: {openPrice?.ToString()}|hight: {hightPrice?.ToString()}|low: {lowPrice.ToString()}|close: {closePrice.ToString()}");
            //                    Console.WriteLine($"=====================================================================================");
            //                }
            //            }

            //            openPrice = tradingPrice;
            //            hightPrice = tradingPrice;
            //            lowPrice = tradingPrice;
            //            turnOver = tradingTurnover;
            //            preSecond = second;
            //            preMilisecond = milisecond;
            //        }
            //    }

            //});

        //};
        return StaticObject.PublicWebsocket;
    }

    private static ConcurrentDictionary<string, Candle> _candles = new ConcurrentDictionary<string, Candle>();
    private static ConcurrentDictionary<string, long> _candle1s = new ConcurrentDictionary<string, long>();
    private static string _websocketUrl = "wss://stream.bybit.com/v5/public/spot"; // URL của websocket Bybit

    private void RunScanner()
    {
        var exitEvent = new ManualResetEvent(false);
        var url = new Uri(_websocketUrl);
        var symbols = StaticObject.Symbols.Where(s => s.MarginTrading == MarginTrading.Both && s.Name.EndsWith("USDT")).Select(c => c.Name).Distinct().ToList();

        using (var client = new WebsocketClient(url))
        {
            client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ReconnectionHappened.Subscribe(info =>
                Console.WriteLine($"Reconnection happened, type: {info.Type}"));

            client.MessageReceived.Subscribe(msg => {

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
                                if (longPercent < (decimal)- 1 && longElastic >= 60)
                                {
                                    Console.WriteLine($"{symbol}|long: {longPercent.ToString("0.00")}%|TP: {longElastic.ToString("0.00")}|Vol: {(candle.Volume / 1000).ToString("0.00")}K");
                                    Console.WriteLine($"{symbol}|open: {candle.Open.ToString()}|hight: {candle.High.ToString()}|low: {candle.Low.ToString()}|close: {candle.Close.ToString()}");
                                    Console.WriteLine($"=====================================================================================");
                                }
                                if (shortPercent > (decimal)1 && shortElastic >= 60)
                                {
                                    Console.WriteLine($"{symbol}|short: {shortPercent.ToString("0.00")}%|TP: {shortElastic.ToString("0.00")}|Vol: {(candle.Volume / 1000).ToString("0.00")}K");
                                    Console.WriteLine($"{symbol}|open: {candle.Open.ToString()}|hight: {candle.High.ToString()}|low: {candle.Low.ToString()}|close: {candle.Close.ToString()}");
                                    Console.WriteLine($"=====================================================================================");
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
            client.Start();

            Task.Run(() => {

                foreach (var symbol in symbols)
                {
                    var messageSubs = $"{{\"op\":\"subscribe\",\"args\":[\"publicTrade.{symbol}\"]}}";
                    client.Send(messageSubs);
                }                 
            
            });

            exitEvent.WaitOne();
        }
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