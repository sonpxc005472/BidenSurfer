namespace BidenSurfer.BotRunner.Services;

using BidenSurfer.Infras;
using BidenSurfer.Infras.Models;
using System.Collections.Generic;
using Bybit.Net.Clients;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using Bybit.Net.Enums;
using Bybit.Net.Objects.Models.V5;
using CryptoExchange.Net.Objects;
using MassTransit;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Helpers;
using BidenSurfer.Infras.Entities;
using System;

public interface IBotService
{
    Task<BybitSocketClient> SubscribeSticker();
    Task SubscribeKline1m();
    Task InitUserApis();
    Task SubscribeOrderChannel();
    Task<bool> TakePlaceOrder(ConfigDto config, decimal currentPrice);
    Task<bool> AmendOrder(ConfigDto config, decimal currentPrice);
    Task<bool> CancelOrder(ConfigDto config, bool isExpired);
}

public class BotService : IBotService
{
    private readonly IConfigService _configService;
    private readonly IUserService _userService;
    private readonly IBus _bus;
    private readonly ITeleMessage _teleMessage;
    private SemaphoreSlim _mutex = new SemaphoreSlim(1);
    public BotService(IConfigService configService, IUserService userService, IBus bus, ITeleMessage teleMessage)
    {
        _configService = configService;
        _userService = userService;
        _bus = bus;
        _teleMessage = teleMessage;
    }

    public async Task<BybitSocketClient> SubscribeSticker()
    {
        var allActiveConfigs = await _configService.GetAllActive();
        var symbols = allActiveConfigs.Where(c => c.IsActive).Select(c => c.Symbol).Distinct().ToList();

        foreach (var symbol in symbols)
        {
            UpdateSubscription candleSubs;
            DateTime preTime = DateTime.Now;
            DateTime preTimeCancel = DateTime.Now;
            decimal prePrice = 0;
            if (!StaticObject.TickerSubscriptions.TryGetValue(symbol, out candleSubs))
            {
                var result = await StaticObject.PublicWebsocket.V5SpotApi.SubscribeToTickerUpdatesAsync(symbol, async data =>
                {
                    if (data != null)
                    {
                        var currentData = data.Data;
                        var currentTime = DateTime.Now;
                        var currentPrice = currentData.LastPrice;

                        await _mutex.WaitAsync();
                        try
                        {
                            allActiveConfigs = StaticObject.AllConfigs;
                            var symbolConfigs = allActiveConfigs.Where(c => c.Symbol == symbol && c.IsActive).ToList();
                            var openScanners = symbolConfigs.Where(x => x.CreatedBy == AppConstants.CreatedByScanner && !string.IsNullOrEmpty(x.OrderId)).ToList();
                            foreach (var symbolConfig in symbolConfigs)
                            {
                                bool isExistedScanner = openScanners.Any(x=>x.UserId == symbolConfig.UserId);
                                bool isLongSide = symbolConfig.PositionSide == AppConstants.LongSide;
                                var existingFilledOrders = symbolConfigs.Where(x => x.UserId == symbolConfig.UserId && x.OrderStatus == 2).ToList();
                                var sideOrderExisted = symbolConfigs.Any(x => x.UserId == symbolConfig.UserId && x.PositionSide != symbolConfig.PositionSide);
                                if ((symbolConfig.CreatedBy != AppConstants.CreatedByScanner || (symbolConfig.CreatedBy == AppConstants.CreatedByScanner && !isExistedScanner)) && !existingFilledOrders.Any() && !sideOrderExisted && string.IsNullOrEmpty(symbolConfig.OrderId))
                                {
                                    prePrice = currentPrice;
                                    //Place order
                                    await TakePlaceOrder(symbolConfig, currentPrice);
                                }
                                else if (!string.IsNullOrEmpty(symbolConfig.OrderId) && symbolConfig.OrderStatus != 2 && !symbolConfig.isClosingFilledOrder)
                                {
                                    // every 2s change order
                                    if ((currentTime - preTime).TotalMilliseconds >= 2000)
                                    {
                                        preTime = currentTime;
                                        var priceDiff = Math.Abs(currentPrice - prePrice) / prePrice * 100;
                                        //Nếu giá dịch chuyển lớn hơn 0.05% so với giá lúc trước thì amend order
                                        if (priceDiff > (decimal)0.05)
                                        {
                                            prePrice = currentPrice;
                                            //Amend order
                                            await AmendOrder(symbolConfig, currentPrice);
                                        }
                                    }
                                }
                                else if (existingFilledOrders != null && existingFilledOrders.Any())
                                {
                                    //Đóng vị thế giá hiện tại nếu mở quá 3s mà chưa đóng được
                                    foreach (var order in existingFilledOrders)
                                    {
                                        if ((currentTime - order.EditedDate.Value).TotalMilliseconds >= 3000)
                                        {
                                            await CloseFilledOrder(order, currentPrice);
                                            order.EditedDate = currentTime;
                                        }
                                    }
                                }
                            }

                            if ((currentTime - preTimeCancel).TotalMilliseconds >= 4000)
                            {
                                preTimeCancel = currentTime;
                                var configExpired = allActiveConfigs.Where(x => (x.IsActive && !string.IsNullOrEmpty(x.OrderId) && x.OrderStatus != 2 && x.EditedDate != null && x.Expire != null && x.Expire.Value != 0 && x.EditedDate.Value.AddMinutes(x.Expire.Value) < currentTime)).ToList();
                                var cancelledConfigs = new List<string>();
                                foreach (var config in configExpired)
                                {
                                    await CancelOrder(config, true);                                    
                                }                                
                            }
                        }
                        finally
                        {
                            _mutex.Release();
                        }
                    }

                });
                if (result.Success)
                {
                    StaticObject.TickerSubscriptions.TryAdd(symbol, result.Data);
                }
            };

        }
        var subsToUnsubs = StaticObject.TickerSubscriptions.Where(o => !symbols.Any(a => a == o.Key)).ToList();
        foreach (var unsub in subsToUnsubs)
        {
            await StaticObject.PublicWebsocket.UnsubscribeAsync(unsub.Value);
            StaticObject.TickerSubscriptions.TryRemove(unsub);
        }
        return StaticObject.PublicWebsocket;
    }

    public async Task<bool> TakePlaceOrder(ConfigDto? config, decimal currentPrice)
    {
        try
        {
            var filledOrderExist = StaticObject.FilledOrders.FirstOrDefault(o => o.CustomId == config.CustomId);
            if (filledOrderExist == null)
            {
                if (config != null)
                {
                    var userSetting = StaticObject.AllUsers.FirstOrDefault(u => u.Id == config.UserId)?.Setting;
                    BybitRestClient api;
                    if (!StaticObject.RestApis.TryGetValue(config.UserId, out api))
                    {
                        api = new BybitRestClient();
                        
                        if (userSetting != null)
                        {
                            api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey, ApiCredentialsType.Hmac));
                            StaticObject.RestApis.TryAdd(config.UserId, api);
                        }
                    }

                    if (api != null)
                    {
                        var orderPriceAndQuantity = CalculateOrderPriceQuantityTP(currentPrice, config);
                        var orderSide = config.PositionSide == AppConstants.ShortSide ? OrderSide.Sell : OrderSide.Buy;
                        if (config.OrderType == (int)OrderTypeEnums.Spot)
                        {
                            orderSide = OrderSide.Buy;
                        }
                        string clientOrderId = Guid.NewGuid().ToString();
                        Console.WriteLine($"Take order {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange}: {DateTime.Now.ToString("dd/MM/yy HH:mm:ss.fff")}");
                        var placedOrder = await api.V5Api.Trading.PlaceOrderAsync
                            (
                                Category.Spot,
                                config.Symbol,
                                orderSide,
                                NewOrderType.Limit,
                                orderPriceAndQuantity.Item2,
                                orderPriceAndQuantity.Item1,
                                config.OrderType == (int)OrderTypeEnums.Margin,
                                clientOrderId: clientOrderId
                            );
                        if (placedOrder.Success)
                        {
                            config.OrderId = placedOrder.Data.OrderId;
                            config.ClientOrderId = clientOrderId;
                            config.TPPrice = orderPriceAndQuantity.Item3;
                            config.OrderStatus = 1;
                            config.TotalQuantity = orderPriceAndQuantity.Item2;
                            _configService.AddOrEditConfig(config);
                        }
                        else
                        {
                            var message = $"Take order {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} error: {placedOrder?.Error?.Message}";
                            Console.WriteLine(message);
                            await _teleMessage.ErrorMessage(config.Symbol, config.OrderChange.ToString(), config.PositionSide, userSetting.TeleChannel, placedOrder?.Error?.Message ?? string.Empty);
                            _configService.UpdateConfig(new List<ConfigDto> { 
                                new ConfigDto
                                {
                                    CustomId = config.CustomId,
                                    IsActive = false
                                }
                            });
                            await _bus.Send(new OffConfigMessage { Configs = new List<string> { config.CustomId } });
                            await _bus.Send(new OnOffConfigMessageScanner
                            {
                                Configs = new List<ConfigDto> {
                                    new ConfigDto{
                                        CustomId = config.CustomId,
                                        IsActive = false,
                                    }
                                }
                            });
                        }
                    }

                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // log error to the telegram channels
            return false;
        }

    }

    public async Task<bool> AmendOrder(ConfigDto config, decimal currentPrice)
    {
        var userSetting = StaticObject.AllUsers.FirstOrDefault(u => u.Id == config.UserId)?.Setting;
        try
        {            
            BybitRestClient api;
            if (!StaticObject.RestApis.TryGetValue(config.UserId, out api))
            {
                api = new BybitRestClient();
                
                api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey));
                StaticObject.RestApis.TryAdd(config.UserId, api);
            }
            var symbol = config.Symbol;
            var orderPriceAndQuantity = CalculateOrderPriceQuantityTP(currentPrice, config);
            var orderPrice = orderPriceAndQuantity.Item1;
            var tpPriceUpdate = orderPriceAndQuantity.Item3;
            if (StaticObject.Kline1mSubscriptions.ContainsKey(symbol))
            {
                var klineData = StaticObject.Kline1mSubscriptions[symbol];
                if ((config.PositionSide == AppConstants.ShortSide && orderPrice <= klineData.OpenPrice) || (config.PositionSide == AppConstants.LongSide && orderPrice >= klineData.OpenPrice))
                {
                    orderPrice = klineData.OpenPrice;
                    var instrumentDetail = StaticObject.Symbols.FirstOrDefault(i => i.Name == config.Symbol);
                    var tpPrice = config.PositionSide == AppConstants.ShortSide ? orderPrice - ((currentPrice * config.OrderChange / 100) * _tp / 100) : orderPrice + ((currentPrice * config.OrderChange / 100) * _tp / 100);
                    tpPriceUpdate = (decimal)(((int)(tpPrice / instrumentDetail?.PriceFilter?.TickSize ?? 1)) * instrumentDetail?.PriceFilter?.TickSize);
                }
            }
            var amendOrder = await api.V5Api.Trading.EditOrderAsync
                (
                    Category.Spot,
                    symbol,
                    config.OrderId,
                    config.ClientOrderId,
                    orderPriceAndQuantity.Item2,
                    orderPrice
                );
            if (amendOrder.Success)
            {
                config.TPPrice = tpPriceUpdate;
                config.OrderStatus = 1;
                config.TotalQuantity = orderPriceAndQuantity.Item2;
                _configService.AddOrEditConfig(config);
                return true;
            }
            else
            {
                var message = $"Amend {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} error: {amendOrder.Error.Message}";
                Console.WriteLine(message);
                await _teleMessage.ErrorMessage(config.Symbol, config.OrderChange.ToString(), config.PositionSide, userSetting.TeleChannel, $"Amend Error: {amendOrder.Error.Message}");
                await CancelOrder(config);
            }
            return false;
        }
        catch (Exception ex)
        {
            var message = $"Amend {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} error: {ex.Message}";
            Console.WriteLine(message);
            await _teleMessage.ErrorMessage(config.Symbol, config.OrderChange.ToString(), config.PositionSide, userSetting.TeleChannel, $"Amend Error: {ex.Message}");
            await CancelOrder(config);
            return false;
        }
    }

    public async Task<bool> CancelOrder(ConfigDto config, bool isExpired = false)
    {
        var userSetting = StaticObject.AllUsers.FirstOrDefault(u => u.Id == config.UserId)?.Setting;
        BybitRestClient api;
        if (!StaticObject.RestApis.TryGetValue(config.UserId, out api))
        {
            api = new BybitRestClient();
            
            if (userSetting != null)
            {
                api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey, ApiCredentialsType.Hmac));
                StaticObject.RestApis.TryAdd(config.UserId, api);
            }
        }

        if (api != null)
        {
            var cancelOrder = await api.V5Api.Trading.CancelOrderAsync
                (
                    Category.Spot,
                    config.Symbol,
                    config.OrderId
                );

            if (cancelOrder.Success)
            {
                var messageSub = isExpired ? $"Expired {config.Expire}m" : $"Cancelled";
                var message = isExpired ? $"{config.Symbol} | {config.PositionSide.ToUpper()}| {config.OrderChange.ToString()} {messageSub}" : $"{config.Symbol} | {config.PositionSide.ToUpper()}| {config.OrderChange.ToString()} {messageSub}";
                Console.WriteLine(message);
                await _teleMessage.OffConfigMessage(config.Symbol, config.OrderChange.ToString(), config.PositionSide, userSetting.TeleChannel, messageSub);
                return true;
            }
        }

        return false;
    }

    public async Task SubscribeKline1m()
    {
        //var configDtos = await _configService.GetAllActive();
        var symbols = StaticObject.Symbols.Where(c => c.MarginTrading == MarginTrading.Both || c.MarginTrading == MarginTrading.UtaOnly).Select(c => c.Name).Distinct().ToList();
        //var unsubsSymbols = symbols.Where(s => !StaticObject.Kline1mSubscriptions.ContainsKey(s)).ToList();
        foreach (var symbol in symbols)
        {
            var result = await StaticObject.PublicWebsocket.V5SpotApi.SubscribeToKlineUpdatesAsync(new List<string> { symbol }, KlineInterval.OneMinute, async data =>
            {
                if (data != null)
                {
                    var klineData = data.Data;
                    foreach (var kline in klineData)
                    {
                        StaticObject.Kline1mSubscriptions[symbol] = kline;
                    }
                }

            });
        }
    }

    public async Task SubscribeOrderChannel()
    {

        foreach (var user in StaticObject.AllUsers)
        {
            BybitSocketClient socket;
            if (!StaticObject.Sockets.TryGetValue(user.Id, out socket))
            {
                socket = new BybitSocketClient();
                var userSetting = user.Setting;
                socket.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey));
                StaticObject.Sockets.TryAdd(user.Id, socket);
            }

            //var activeConfigs = userConfigs.Where(c => c.IsActive).ToList();
            UpdateSubscription orderSub;
            if (!StaticObject.OrderSubscriptions.TryGetValue(user.Id, out orderSub))
            {
                var result = await socket.V5PrivateApi.SubscribeToOrderUpdatesAsync(async (data) =>
                {
                    var updatedDatas = data.Data.ToList();
                    foreach (var updatedData in updatedDatas)
                    {
                        var orderState = updatedData?.Status;
                        var orderId = updatedData?.OrderId;
                        if(orderState != Bybit.Net.Enums.V5.OrderStatus.New && orderState != Bybit.Net.Enums.V5.OrderStatus.Created)
                            Console.WriteLine($"{updatedData?.Symbol} | {orderState} | {orderId}");
                        var config = StaticObject.AllConfigs.FirstOrDefault(c => c.OrderId == orderId);
                        if(config == null)
                        {
                            Console.WriteLine("Null order: " + orderId);
                            continue;
                        }
                        if (orderState == Bybit.Net.Enums.V5.OrderStatus.PartiallyFilled)
                        {                            
                            var closingOrder = StaticObject.FilledOrders.FirstOrDefault(c => c.OrderId == orderId && c.OrderStatus == 2);
                            if (closingOrder == null)
                            {
                                Console.WriteLine($"{updatedData?.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange.ToString()} - PartiallyFilled");

                                await _teleMessage.FillMessage(updatedData.Symbol, config.OrderChange.ToString(), config.PositionSide, user.Setting.TeleChannel, false, updatedData.QuantityFilled ?? 0, config.TotalQuantity ?? 0, updatedData.AveragePrice ?? 0);
                                BybitRestClient api;
                                if (!StaticObject.RestApis.TryGetValue(user.Id, out api))
                                {
                                    api = new BybitRestClient();
                                    var userSetting = user.Setting;
                                    api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey));
                                    StaticObject.RestApis.TryAdd(user.Id, api);
                                }
                                var cancelOrder = await api.V5Api.Trading.CancelOrderAsync
                                (
                                    Category.Spot,
                                    updatedData?.Symbol,
                                    updatedData?.OrderId
                                );
                                if (cancelOrder.Success)
                                {
                                    config.isClosingFilledOrder = true;
                                    await ClosePosition(updatedData, user);                                    
                                }
                            }

                        }
                        else if (orderState == Bybit.Net.Enums.V5.OrderStatus.Filled)
                        {                            
                            var closingOrder = StaticObject.FilledOrders.FirstOrDefault(c => c.OrderId == orderId && c.OrderStatus == 2);
                            if (closingOrder == null)
                            {
                                config.isClosingFilledOrder = true;
                                Console.WriteLine($"{updatedData?.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange.ToString()} - Filled");
                                await _teleMessage.FillMessage(updatedData.Symbol, config.OrderChange.ToString(), config.PositionSide, user.Setting.TeleChannel, true, updatedData.QuantityFilled ?? 0, config.TotalQuantity ?? 0, updatedData.AveragePrice ?? 0);

                                await ClosePosition(updatedData, user);                                                               
                            }
                            else
                            {
                                var openPrice = closingOrder.FilledPrice ?? 0;
                                var closePrice = updatedData?.AveragePrice ?? 0;
                                var filledQuantity = updatedData?.QuantityFilled ?? 0;
                                var pnlCash = Math.Round(Math.Abs((openPrice - closePrice) * filledQuantity), 2);
                                var pnlPercent = Math.Round((pnlCash / (openPrice * filledQuantity)) * 100, 2);
                                var pnlText = pnlCash > 0 ? "WIN" : "LOSE";
                                Console.WriteLine($"{updatedData?.Symbol}|{closingOrder.OrderChange}|{pnlText}|PNL: ${pnlCash.ToString("0.00")} {pnlPercent.ToString("0.00")}%");
                                
                                StaticObject.FilledOrders.TryTake(out closingOrder);
                                config.OrderId = string.Empty;
                                config.ClientOrderId = string.Empty;
                                config.OrderStatus = null;
                                config.IsActive = !(config.CreatedBy == AppConstants.CreatedByScanner && pnlCash <= 0) || pnlCash > 0;
                                config.isClosingFilledOrder = false;
                                config.EditedDate = DateTime.Now;
                                var amountIncrease = config.Amount;
                                if(pnlCash <= 0 && config.CreatedBy != AppConstants.CreatedByScanner && config.IncreaseOcPercent != null && config.IncreaseOcPercent > 0)
                                {
                                    config.OrderChange = config.OrderChange + (config.OrderChange * config.IncreaseOcPercent.Value / 100);
                                }    
                                if(config.IncreaseAmountPercent != null && config.IncreaseAmountPercent > 0)
                                {
                                    amountIncrease = config.Amount + (config.OriginAmount.Value * config.IncreaseAmountPercent.Value / 100);
                                }
                                if(config.AmountLimit != null && config.AmountLimit > 0)
                                {
                                    amountIncrease = amountIncrease > config.AmountLimit ? config.AmountLimit.Value : amountIncrease;
                                }    
                                
                                config.Amount = config.CreatedBy == AppConstants.CreatedByScanner && pnlCash <= 0? config.OriginAmount.Value : amountIncrease;
                                                                
                                if (pnlCash <= 0 && config.CreatedBy == AppConstants.CreatedByScanner)
                                {
                                    _configService.UpdateConfig(new List<ConfigDto> {
                                        new ConfigDto
                                        {
                                            CustomId = config.CustomId,
                                            IsActive = false
                                        }
                                    });
                                    await _bus.Send(new OffConfigMessage
                                    {
                                        Configs = new List<string>
                                            {
                                                config.CustomId
                                            }
                                    });
                                }
                                else
                                {
                                    _configService.UpdateConfig(new List<ConfigDto> {
                                        config
                                    });
                                }
                                _configService.UpsertWinLose(config, pnlCash > 0);
                                var configWin = _configService.GetWinLose(config);
                                await _teleMessage.PnlMessage(updatedData.Symbol, config.OrderChange.ToString(), config.PositionSide, user.Setting.TeleChannel, pnlCash > 0, pnlCash, pnlPercent, configWin.Win, configWin.Total);
                            }
                        }
                        else if(orderState == Bybit.Net.Enums.V5.OrderStatus.Cancelled)
                        {
                            var customId = config.CustomId;

                            _configService.UpdateConfig(new List<ConfigDto> {
                                new ConfigDto
                                {
                                    CustomId = customId,
                                    IsActive = false
                                }
                            });

                            await _bus.Send(new OffConfigMessage
                            {
                                Configs = new List<string>
                                {
                                    customId
                                }
                            });
                        }    
                    }

                });

                if (result.Success)
                {
                    StaticObject.OrderSubscriptions.TryAdd(user.Id, result.Data);
                }
            }
            
        } 

    }

    public async Task InitUserApis()
    {
        if (!StaticObject.Symbols.Any())
        {
            var publicApi = new BybitRestClient();
            var spotSymbols = (await publicApi.V5Api.ExchangeData.GetSpotSymbolsAsync()).Data.List;
            StaticObject.Symbols = spotSymbols.ToList();
        }
        foreach (var user in StaticObject.AllUsers)
        {
            if (user.Status == 1 && !string.IsNullOrEmpty(user.Setting?.ApiKey) && !string.IsNullOrEmpty(user.Setting.SecretKey))
            {
                BybitRestClient api;
                if (!StaticObject.RestApis.TryGetValue(user.Id, out api))
                {
                    var userSetting = user.Setting;
                    api = new BybitRestClient(options =>
                    {
                        options.ApiCredentials = new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey);
                    });

                    StaticObject.RestApis.TryAdd(user.Id, api);
                }
                var marginSymbols = StaticObject.Symbols.Where(c => c.MarginTrading == MarginTrading.Both || c.MarginTrading == MarginTrading.UtaOnly).Select(c => new { c.Name, c.BaseAsset }).Distinct().ToList();

                foreach (var symbol in marginSymbols)
                {
                    var rs = await api.V5Api.Account.SetCollateralAssetAsync(symbol.BaseAsset, true);
                    await api.V5Api.Account.SetLeverageAsync(Category.Spot, symbol.Name, 5, 5);
                }
            }
        }

    }

    private async Task ClosePosition(BybitOrderUpdate orderUpdate, UserDto user)
    {
        var orderId = orderUpdate?.OrderId;
        var configToUpdate = StaticObject.AllConfigs.FirstOrDefault(c => c.OrderId == orderId);
        try
        {            
            BybitRestClient api;
            if (!StaticObject.RestApis.TryGetValue(user.Id, out api))
            {
                api = new BybitRestClient();
                var userSetting = user.Setting;
                api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey));
                StaticObject.RestApis.TryAdd(user.Id, api);
            }
            var ordSide = orderUpdate?.Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
            var orderPrice = configToUpdate?.TPPrice;
            var clientOrderId = Guid.NewGuid().ToString();
            var placedOrder = await api.V5Api.Trading.PlaceOrderAsync
            (
                Category.Spot,
                orderUpdate?.Symbol,
                ordSide,
                NewOrderType.Limit,
                orderUpdate?.QuantityFilled ?? 0,
                orderPrice,
                false,
                clientOrderId: clientOrderId
            );

            if (placedOrder.Success && configToUpdate != null)
            {
                Console.WriteLine($"Closing Filled Order: {orderUpdate.Symbol}|{configToUpdate.PositionSide}| {configToUpdate.OrderChange}|${orderPrice}- OrderId: {placedOrder.Data.OrderId}");
                configToUpdate.OrderId = placedOrder.Data.OrderId;
                configToUpdate.ClientOrderId = clientOrderId;
                configToUpdate.FilledPrice = orderUpdate.AveragePrice;
                configToUpdate.FilledQuantity = orderUpdate.QuantityFilled;
                configToUpdate.OrderStatus = 2;
                configToUpdate.EditedDate = DateTime.Now;
                StaticObject.FilledOrders.Add(configToUpdate);
                _configService.AddOrEditConfig(configToUpdate);
            }
            else
            {
                Console.WriteLine($"Close {orderUpdate.Symbol} error: {placedOrder?.Error?.Message}");
            }
            
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Close Filled Order {orderUpdate.Symbol}|{orderUpdate.Side}|{configToUpdate.OrderChange} Error: {ex.ToString()}");
        }
        
    }

    private async Task<bool> CloseFilledOrder(ConfigDto config, decimal currentPrice)
    {
        try
        {
            BybitRestClient api;
            if (!StaticObject.RestApis.TryGetValue(config.UserId, out api))
            {
                api = new BybitRestClient();
                var userSetting = StaticObject.AllUsers.FirstOrDefault(u => u.Id == config.UserId)?.Setting;
                api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey));
                StaticObject.RestApis.TryAdd(config.UserId, api);
            }
            var symbol = config.Symbol;
            var instrumentDetail = StaticObject.Symbols.FirstOrDefault(i => i.Name == config.Symbol);
            var orderPriceWithTicksize = ((int)(currentPrice / instrumentDetail?.PriceFilter?.TickSize ?? 1)) * instrumentDetail?.PriceFilter?.TickSize;

            var amendOrder = await api.V5Api.Trading.EditOrderAsync
                (
                    Category.Spot,
                    symbol,
                    config.OrderId,
                    config.ClientOrderId,
                    config.FilledQuantity,
                    orderPriceWithTicksize
                );
            if(!amendOrder.Success)
            {
                Console.WriteLine($"Close Filled Order {config.Symbol}|{config.PositionSide}|{config.OrderChange}\nError: {amendOrder.Error.Message}");
                config.isClosingFilledOrder = false;
                config.OrderStatus = null;
            }
            return amendOrder.Success;
        }
        catch (Exception ex)
        {
            // Log error to telegram
            Console.WriteLine($"Close Filled Order {config.Symbol}|{config.PositionSide}|{config.OrderChange}\nError: {ex.ToString()}");
            return false;
        }
    }


    private readonly decimal _tp = 70;
    private (decimal, decimal, decimal) CalculateOrderPriceQuantityTP(decimal currentPrice, ConfigDto config)
    {
        var orderSide = config.PositionSide == AppConstants.ShortSide ? OrderSide.Sell : OrderSide.Buy;
        var orderPrice = config.PositionSide == AppConstants.ShortSide ? currentPrice + (currentPrice * config.OrderChange / 100) : currentPrice - (currentPrice * config.OrderChange / 100);
        var instrumentDetail = StaticObject.Symbols.FirstOrDefault(i => i.Name == config.Symbol);
        var orderPriceWithTicksize = ((int)(orderPrice / instrumentDetail?.PriceFilter?.TickSize ?? 1)) * instrumentDetail?.PriceFilter?.TickSize;
        var quantity = config.Amount / orderPriceWithTicksize;
        var quantityWithTicksize = ((int)(quantity / instrumentDetail?.LotSizeFilter?.BasePrecision ?? 1)) * instrumentDetail?.LotSizeFilter?.BasePrecision;
        var tpPrice = config.PositionSide == AppConstants.ShortSide ? orderPrice - ((currentPrice * config.OrderChange / 100) * _tp / 100) : orderPrice + ((currentPrice * config.OrderChange / 100) * _tp / 100);
        var tpPriceWithTicksize = ((int)(tpPrice / instrumentDetail?.PriceFilter?.TickSize ?? 1)) * instrumentDetail?.PriceFilter?.TickSize;


        return (orderPriceWithTicksize ?? 0, quantityWithTicksize ?? 0, tpPriceWithTicksize ?? 0);
    }
}