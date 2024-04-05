namespace BidenSurfer.BotRunner.Services;

using BidenSurfer.Infras;
using BidenSurfer.Infras.Models;
using System.Collections.Generic;
using Bybit.Net.Clients;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using Bybit.Net.Enums;
using BidenSurfer.Infras.Entities;
using Bybit.Net.Objects.Models.V5;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;

public interface IBotService
{
    Task<BybitSocketClient> SubscribeSticker();
    Task SubscribeKline1m();
    Task InitUserApis();
    Task SubscribeOrderChannel();
    Task<bool> TakePlaceOrder(ConfigDto config, decimal currentPrice);
    Task<bool> CloseFilledOrder(ConfigDto config, decimal quantity);
    Task<bool> AmendOrder(ConfigDto config, decimal currentPrice);
    Task<bool> DeleteOrder(ConfigDto config);
}

public class BotService : IBotService
{
    private readonly IConfigService _configService;
    private readonly IUserService _userService;
    private static object _locker = new object();
    private SemaphoreSlim _mutex = new SemaphoreSlim(1);
    public BotService(IConfigService configService, IUserService userService)
    {
        _configService = configService;
        _userService = userService;
    }

    public async Task<BybitSocketClient> SubscribeSticker()
    {
        var allActiveConfigs = _configService.GetAllActive();
        var symbols = allActiveConfigs.Where(c => c.IsActive).Select(c => c.Symbol).Distinct().ToList();

        foreach (var symbol in symbols)
        {
            UpdateSubscription candleSubs;
            DateTime preTime = DateTime.Now;
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
                            var userConfigs = allActiveConfigs.Where(c => c.Symbol == symbol).ToList();
                            foreach (var userConfig in userConfigs)
                            {
                                bool isLongSide = userConfig.PositionSide == AppConstants.LongSide;
                                var userOrders = StaticObject.FilledOrders.Where(x => x.UserId == userConfig.UserId).ToList();
                                var existingFilledOrders = userOrders.Where(x => x.Symbol == userConfig.Symbol && x.PositionSide == userConfig.PositionSide).FirstOrDefault();
                                if (existingFilledOrders == null && string.IsNullOrEmpty(userConfig.OrderId))
                                {
                                    prePrice = currentPrice;
                                    //Place order
                                    await TakePlaceOrder(userConfig, currentPrice);
                                }
                                else if (existingFilledOrders == null)
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
                                            await AmendOrder(userConfig, currentPrice);
                                        }
                                    }                                    
                                }
                            }
                        }
                        finally
                        {
                            _mutex.Release();
                        }
                    }
                    
                });
                if(result.Success)
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
                    BybitRestClient api;
                    if (!StaticObject.RestApis.TryGetValue(config.UserId, out api))
                    {
                        api = new BybitRestClient();
                        var userSetting = config.UserDto.Setting;
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
                            _configService.AddOrEditConfig(config);
                        }
                        else
                        {
                            Console.WriteLine($"Take order {config.Symbol} error: {placedOrder.Error}");
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
        try
        {
            BybitRestClient api;
            if (!StaticObject.RestApis.TryGetValue(config.UserId, out api))
            {
                api = new BybitRestClient();
                var userSetting = config.UserDto.Setting;
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
                _configService.AddOrEditConfig(config);
                return true;
            }
            else
            {
                Console.WriteLine($"Take order {config.Symbol} error: {amendOrder.Error}");
            }
            return false;
        }
        catch (Exception ex)
        {
            // Log error to telegram
            return false;
        }
    }

    public async Task<bool> DeleteOrder(ConfigDto config)
    {
        BybitRestClient api;
        if (!StaticObject.RestApis.TryGetValue(config.UserId, out api))
        {
            api = new BybitRestClient();
            var userSetting = config.UserDto.Setting;
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
                    config.OrderId,
                    config.ClientOrderId
                );

            if (cancelOrder != null && cancelOrder.Success)
            {
                _configService.DeleteConfig(config.CustomId);
            }
        }

        return true;
    }

    public async Task<bool> CloseFilledOrder(ConfigDto config, decimal quantity)
    {
        BybitRestClient api;
        if (!StaticObject.RestApis.TryGetValue(config.UserId, out api))
        {
            api = new BybitRestClient();
            var userSetting = config.UserDto.Setting;
            if (userSetting != null)
            {
                api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey, ApiCredentialsType.Hmac));
                StaticObject.RestApis.TryAdd(config.UserId, api);
            }
        }

        if (api != null)
        {
            var orderSide = config.PositionSide == AppConstants.ShortSide ? OrderSide.Buy : OrderSide.Sell;
            if (config.OrderType == (int)OrderTypeEnums.Spot)
            {
                orderSide = OrderSide.Sell;
            }
            await api.V5Api.Trading.PlaceOrderAsync
                (
                    Category.Spot,
                    config.Symbol,
                    orderSide,
                    NewOrderType.Limit,
                    quantity,
                    config.TPPrice,
                    config.OrderType == (int)OrderTypeEnums.Margin,
                    clientOrderId: config.ClientOrderId
                );
            StaticObject.FilledOrders.Add(config);
        }

        return true;
    }

    public async Task SubscribeKline1m()
    {
        var configDtos = _configService.GetAllActive();
        var symbols = configDtos.Where(c => c.IsActive).Select(c => c.Symbol).Distinct().ToList();
        var unsubsSymbols = symbols.Where(s => !StaticObject.Kline1mSubscriptions.ContainsKey(s)).ToList();
        foreach (var symbol in unsubsSymbols)
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
        //var allConfigs = await _configService.GetAllActive();
        var allUsers = await _userService.GetAllActive();
        //if(allConfigs.Any())
        //{
            foreach (var user in allUsers)
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
                UpdateSubscription orderId;
                if (!StaticObject.OrderSubscriptions.TryGetValue(user.Id, out orderId))
                {
                    var result = await socket.V5PrivateApi.SubscribeToOrderUpdatesAsync(async (data) =>
                    {
                        var updatedDatas = data.Data.ToList();
                        foreach (var updatedData in updatedDatas)
                        {
                            var orderState = updatedData?.Status;
                            var orderId = updatedData?.OrderId;
                            if (orderState == Bybit.Net.Enums.V5.OrderStatus.PartiallyFilled)
                            {
                                Console.WriteLine($"{updatedData?.Symbol}-PartiallyFilled");
                                var closingOrder = StaticObject.FilledOrders.FirstOrDefault(c => c.OrderId == orderId && c.OrderStatus == 2);
                                if(closingOrder == null)
                                {
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
                                        updatedData?.OrderId,
                                        updatedData?.ClientOrderId
                                    );
                                    if (cancelOrder.Success)
                                    {
                                        var (placedOrder, configToUpdate) = await ClosePosition(updatedData, user);
                                        if (placedOrder.Success && configToUpdate != null)
                                        {
                                            configToUpdate.FilledPrice = updatedData.AveragePrice;
                                            configToUpdate.FilledQuantity = updatedData.QuantityFilled;
                                            configToUpdate.OrderStatus = 2;
                                            StaticObject.FilledOrders.Add(configToUpdate);
                                            configToUpdate.ClientOrderId = string.Empty;
                                            configToUpdate.OrderId = string.Empty;
                                            _configService.AddOrEditConfig(configToUpdate);
                                        }
                                    }
                                }
                                
                            }
                            else if (orderState == Bybit.Net.Enums.V5.OrderStatus.Filled)
                            {
                                Console.WriteLine($"{updatedData?.Symbol}-Filled");
                                var closingOrder = StaticObject.FilledOrders.FirstOrDefault(c => c.OrderId == orderId && c.OrderStatus == 2);
                                if (closingOrder == null)
                                {                                    
                                    var (placedOrder, configToUpdate) = await ClosePosition(updatedData, user);
                                    if (placedOrder.Success && configToUpdate != null)
                                    {
                                        configToUpdate.FilledPrice = updatedData.AveragePrice;
                                        configToUpdate.FilledQuantity = updatedData.QuantityFilled;
                                        configToUpdate.OrderStatus = 2;
                                        StaticObject.FilledOrders.Add(configToUpdate);
                                        configToUpdate.ClientOrderId = string.Empty;
                                        configToUpdate.OrderId = string.Empty;
                                        _configService.AddOrEditConfig(configToUpdate);
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Close {updatedData.Symbol} error: {placedOrder.Error}");   
                                    }
                                }
                                else
                                {
                                    var openPrice = closingOrder.FilledPrice;
                                    var closePrice = updatedData.AveragePrice;
                                    var filledQuantity = updatedData.QuantityFilled;
                                    var pnlCash = closingOrder.PositionSide == AppConstants.LongSide ? (closePrice - openPrice)*filledQuantity : (openPrice - closePrice)*filledQuantity;
                                    var pnlPercent = pnlCash / (openPrice * filledQuantity) * 100;
                                    var pnlText = pnlCash > 0 ? "Win" : "Lose";
                                    Console.WriteLine($"{updatedData?.Symbol}|{pnlText}|PNL: {pnlCash}-{pnlPercent}");
                                    StaticObject.FilledOrders.TryTake(out closingOrder);
                                }
                            }
                        } 

                    });

                    if(result.Success)
                    {
                        StaticObject.OrderSubscriptions.TryAdd(user.Id, result.Data);
                    }
                }
                //var subsToUnsubs = StaticObject.OrderSubscriptions.Where(o => !allUsers.Any(a => a.Id == o.Key)).ToList();
                //foreach (var unsub in subsToUnsubs)
                //{
                //    await socket.UnsubscribeAsync(unsub.Value);
                //    StaticObject.OrderSubscriptions.TryRemove(unsub);
                //}
            }
        //}    
        
    }

    public async Task InitUserApis()
    {
        var users = await _userService.GetAllActive();

        foreach (var user in users)
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
                var userConfigs = _configService.GetByUserId(user.Id);
                var marginSymbols = userConfigs.Where(c => c.IsActive && c.OrderType == (int)OrderTypeEnums.Margin).Select(c => c.Symbol).Distinct().ToList();

                foreach (var symbol in marginSymbols)
                {
                    await api.V5Api.Account.SetCollateralAssetAsync(symbol, true);
                    await api.V5Api.Account.SetLeverageAsync(Bybit.Net.Enums.Category.Spot, symbol, 5, 5);
                }
            }
        }

        if (!StaticObject.Symbols.Any())
        {
            var publicApi = new BybitRestClient();
            var spotSymbols = (await publicApi.V5Api.ExchangeData.GetSpotSymbolsAsync()).Data.List;
            StaticObject.Symbols = spotSymbols.ToList();
        }
    }

    private async Task<(WebCallResult<BybitOrderId>?, ConfigDto?)> ClosePosition(BybitOrderUpdate orderUpdate, UserDto user)
    {
        var orderId = orderUpdate?.OrderId;
        var existedOrder = StaticObject.FilledOrders.FirstOrDefault(c => c.OrderId == orderId);
        var allConfigs = _configService.GetAllActive();
        var configToUpdate = allConfigs.FirstOrDefault(c => c.OrderId == orderId);
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

        var placedOrder = await api.V5Api.Trading.PlaceOrderAsync
        (
            Category.Spot,
            orderUpdate?.Symbol,
            ordSide,
            NewOrderType.Limit,
            orderUpdate?.QuantityFilled ?? 0,
            orderPrice,
            orderUpdate?.IsLeverage,
            clientOrderId: orderUpdate?.ClientOrderId
        );
        return (placedOrder, configToUpdate);
    }

    private readonly decimal _tp = 20;
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