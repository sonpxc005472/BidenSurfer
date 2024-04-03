namespace BidenSurfer.Bot.Services;

using BidenSurfer.Infras;
using BidenSurfer.Infras.Models;
using System.Collections.Generic;
using System.Text.Json;
using BidenSurfer.Infras.Contracts;
using ApiSharp.WebSocket;
using Bybit.Net.Clients;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using OKX.Api.Enums;
using Bybit.Net.Enums;
using BidenSurfer.Infras.Entities;

public interface IBotService
{
    Task<BybitSocketClient> SubscribeSticker(List<Config> configs);
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

    public BotService(IConfigService configService, IUserService userService)
    {
        _configService = configService;
        _userService = userService;
    }

    public async Task<BybitSocketClient> SubscribeSticker(List<Config> configs = null)
    {
        var configDtos = await _configService.GetAllActive();
        var allUsers = await _userService.GetAllActive();
        var symbols = configDtos.Where(c => c.IsActive).Select(c => c.Symbol).Distinct().ToList();

        await InitialUserApis(allUsers.ToList());

        foreach (var symbol in symbols)
        {
            UpdateSubscription candleSubs;
            if (!StaticObject.Subscriptions.TryGetValue(symbol, out candleSubs))
            {
                var result = await StaticObject.PublicWebsocket.V5SpotApi.SubscribeToTickerUpdatesAsync(symbol, data =>
                {
                    if (data != null)
                    {
                        var userConfigs = configDtos.Where(c => c.Symbol == symbol).ToList();
                        foreach (var userConfig in userConfigs)
                        {
                            bool isLongSide = userConfig.PositionSide == Constants.LongSide;
                            var userOrders = StaticObject.FilledOrders.Where(x => x.UserId == userConfig.UserId).ToList();
                            var existingOrders = userOrders.Where(x => x.Symbol == userConfig.Symbol && x.PositionSide == userConfig.PositionSide).ToList();
                            //var partialOrd = existingOrders?.OrderByDescending(x => x.CreatedDate)?.FirstOrDefault(x => x.Status == (int)OrderStatusEnums.Partial);
                            //if (partialOrd != null)
                            //{
                            //    BybitRestClient api;
                            //    if (!StaticObject.RestApis.TryGetValue(userConfig.UserId, out api))
                            //    {
                            //        api = new BybitRestClient();
                            //        var userSetting = userConfig.UserDto.Setting;
                            //        api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey));
                            //        StaticObject.RestApis.TryAdd(userConfig.UserId, api);
                            //    }

                            //    var ordSide = isLongSide ? OrderSide.Sell : OrderSide.Buy;
                            //    var orderPrice = isLongSide ? partialOrd.FillPrice + ((decimal)userConfig.TakeProfit / 100 * userConfig.OrderChange / 100 * partialOrd.FillPrice) : partialOrd.FillPrice - ((decimal)userConfig.TakeProfit / 100 * userConfig.OrderChange / 100 * partialOrd.FillPrice);
                            //    var instrumentDetail = StaticObject.Instruments.FirstOrDefault(i => i.Instrument == userConfig.Symbol);
                            //    var orderPrice2 = ((int)(orderPrice / instrumentDetail.TickSize)) * instrumentDetail.TickSize;

                            //    var cancelRs = await api.OrderBookTrading.Trade.CancelOrderAsync
                            //        (
                            //            userConfig.Symbol,
                            //            long.Parse(partialOrd.ExternalOrderId)
                            //        );
                            //    if (!cancelRs.Success)
                            //        return;
                            //    StaticObject.FilledOrders.TryTake(out partialOrd);
                            //    //If current price is going toward TP price more than 20%, place order to reduce position and cancel current order
                            //    if ((isLongSide && (data.Close - partialOrd.FillPrice) / partialOrd.FillPrice * 100 > 20) || (!isLongSide && (partialOrd.FillPrice - data.Close) / partialOrd.FillPrice * 100 > 20))
                            //    {

                            //        var placedOrder = await api.OrderBookTrading.Trade.PlaceOrderAsync
                            //        (
                            //            userConfig.Symbol,
                            //            OkxTradeMode.Cross,
                            //            ordSide,
                            //            isLongSide ? OkxPositionSide.Long : OkxPositionSide.Short,
                            //            OkxOrderType.LimitOrder,
                            //            partialOrd.Quantity ?? 0,
                            //            orderPrice2,
                            //            reduceOnly: true
                            //        );
                            //        if (!placedOrder.Success) return;
                            //        var order = new OrderDto
                            //        {
                            //            ExternalOrderId = placedOrder.Data.OrderId?.ToString() ?? string.Empty,
                            //            Symbol = userConfig.Symbol,
                            //            CandleStick = userConfig.CandleStick,
                            //            Amount = userConfig.Amount,
                            //            PositionSide = userConfig.PositionSide,
                            //            Status = (int)OrderStatusEnums.Completed,
                            //            UserId = userConfig.UserId,
                            //            CreatedDate = data.Time,
                            //            TpPrice = orderPrice2,
                            //            Quantity = partialOrd.Quantity
                            //        };
                            //        StaticObject.FilledOrders.Add(order);
                            //    }
                            //}
                            //var uncompletedOrd = existingOrders.FirstOrDefault(x => x.CreatedDate == data.Time);
                            //if (uncompletedOrd != null)
                            //    continue;

                            //var getOrders = existingOrders.Where(x => x.CreatedDate != data.Time).ToList();
                            //await DeleteOrAmendOrder(getOrders, data, userConfig);
                            //var existingCompletedOrder = getOrders.FirstOrDefault(x => x.Status == (int)OrderStatusEnums.Completed);
                            //if (existingCompletedOrder != null)
                            //    continue;
                            //if (userConfig.Extend <= 0)
                            //{
                            //    //Place order
                            //    await TakePlaceOrder(userConfig, data);
                            //}
                            //else
                            //{
                            //    var extendExpected = ((decimal)userConfig.Extend / 100) * userConfig.OrderChange;
                            //    if ((isLongSide && longshort.Item1 >= extendExpected && longshort.Item1 < userConfig.OrderChange) || (!isLongSide && longshort.Item2 >= extendExpected && longshort.Item2 < userConfig.OrderChange))
                            //    {
                            //        //Place order
                            //        await TakePlaceOrder(userConfig, data);
                            //    }
                            //}
                        }

                    }
                });
                StaticObject.Subscriptions.TryAdd(symbol, result.Data);
            };

        }
        var subsToUnsubs = StaticObject.Subscriptions.Where(o => !symbols.Any(a => a == o.Key)).ToList();
        foreach (var unsub in subsToUnsubs)
        {
            await StaticObject.PublicWebsocket.UnsubscribeAsync(unsub.Value);
            StaticObject.Subscriptions.TryRemove(unsub);
        }
        await SubscribeOrderChannel();
        return StaticObject.PublicWebsocket;
    }

    public async Task<bool> TakePlaceOrder(ConfigDto? config, decimal currentPrice)
    {
        try
        {
            var filledOrderExist = StaticObject.FilledOrders.FirstOrDefault(o => o.Id == config.Id);
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
                        var orderSide = config.PositionSide == Constants.ShortSide ? OrderSide.Sell : OrderSide.Buy;
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

                        config.OrderId = clientOrderId;
                        config.TPPrice = orderPriceAndQuantity.Item3;
                        _configService.AddOrEditConfig(config);
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
    private readonly decimal _tp = 70;
    private (decimal, decimal, decimal) CalculateOrderPriceQuantityTP(decimal currentPrice, ConfigDto config)
    {
        var orderSide = config.PositionSide == Constants.ShortSide ? OrderSide.Sell : OrderSide.Buy;
        var orderPrice = config.PositionSide == Constants.ShortSide ? currentPrice + (currentPrice * config.OrderChange / 100) : currentPrice - (currentPrice * config.OrderChange / 100);
        var instrumentDetail = StaticObject.Symbols.FirstOrDefault(i => i.Name == config.Symbol);
        var orderPriceWithTicksize = ((int)(orderPrice / instrumentDetail?.PriceFilter?.TickSize ?? 1)) * instrumentDetail?.PriceFilter?.TickSize;
        var quantity = config.Amount / orderPrice;
        var quantityWithTicksize = ((int)(quantity / instrumentDetail?.PriceFilter?.TickSize ?? 1)) * instrumentDetail?.PriceFilter?.TickSize;
        var tpPrice = config.PositionSide == Constants.ShortSide ? orderPrice - ((currentPrice * config.OrderChange / 100) * _tp / 100) : orderPrice + ((currentPrice * config.OrderChange / 100) * _tp / 100);
        var tpPriceWithTicksize = ((int)(tpPrice / instrumentDetail?.PriceFilter?.TickSize ?? 1)) * instrumentDetail?.PriceFilter?.TickSize;

        return (orderPriceWithTicksize ?? 0, quantityWithTicksize ?? 0, tpPriceWithTicksize ?? 0);
    }

    private async Task InitialUserApis(List<UserDto> users)
    {
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
                    await api.V5Api.Account.SetLeverageAsync(Bybit.Net.Enums.Category.Spot, symbol, 10, 10);
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

    private readonly string _apiAddress = "https://api.bybit.com";

    public async Task SubscribeOrderChannel()
    {
        var allConfigs = await _configService.GetAllActive();
        var allUsers = await _userService.GetAllActive();

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

            var userConfigs = _configService.GetByUserId(user.Id);
            var activeConfigs = userConfigs.Where(c => c.IsActive).ToList();
            foreach (var config in activeConfigs)
            {
                UpdateSubscription orderId;
                if (!StaticObject.OrderSubscriptions.TryGetValue(user.Id, out orderId))
                {
                    var result = await socket.V5PrivateApi.SubscribeToOrderUpdatesAsync(async (data) =>
                    {
                        var updatedData = data.Data.FirstOrDefault();
                        var orderState = updatedData?.Status;

                        if (updatedData?.Symbol == config.Symbol)
                        {
                            if (orderState == Bybit.Net.Enums.V5.OrderStatus.PartiallyFilled)
                            {
                                //string orderIdStr = data.OrderId?.ToString() ?? string.Empty;
                                //var existingOrd = StaticObject.FilledOrders.FirstOrDefault(x => x.ExternalOrderId == orderIdStr);
                                //bool removed;
                                //if (existingOrd != null)
                                //{
                                //    removed = StaticObject.FilledOrders.TryTake(out existingOrd);
                                //}
                                //var order = new OrderDto
                                //{
                                //    ExternalOrderId = data.OrderId.ToString(),
                                //    Symbol = config.Symbol,
                                //    CandleStick = config.CandleStick,
                                //    Amount = config.Amount,
                                //    PositionSide = config.PositionSide,
                                //    Status = (int)OrderStatusEnums.Partial,
                                //    UserId = config.UserId,
                                //    CreatedDate = data.CreateTime,
                                //    Quantity = data.AccumulatedFillQuantity,
                                //    FillPrice = data.FillPrice
                                //};
                                //StaticObject.FilledOrders.Add(order);
                            }
                            else if (orderState == Bybit.Net.Enums.V5.OrderStatus.Filled)
                            {
                                //BybitRestClient api;
                                //if (!StaticObject.OkxApis.TryGetValue(user.Id, out api))
                                //{
                                //    api = new BybitRestClient();
                                //    var userSetting = user.Setting;
                                //    api.SetApiCredentials(userSetting.ApiKey, userSetting.SecretKey, userSetting.PassPhrase);
                                //    StaticObject.OkxApis.TryAdd(user.Id, api);
                                //}
                                //var ordSide = data.OrderSide == OkxOrderSide.Buy ? OkxOrderSide.Sell : OkxOrderSide.Buy;
                                //var orderPrice = data.PositionSide == OkxPositionSide.Long ? data.FillPrice + ((decimal)config.TakeProfit / 100 * config.OrderChange / 100 * data.FillPrice) : data.FillPrice - ((decimal)config.TakeProfit / 100 * config.OrderChange / 100 * data.FillPrice);
                                //var instrumentDetail = StaticObject.Instruments.FirstOrDefault(i => i.Instrument == config.Symbol);
                                //var orderPrice2 = ((int)(orderPrice / instrumentDetail.TickSize)) * instrumentDetail.TickSize;
                                //decimal convert;
                                //if (!StaticObject.ContractToCurrencies.TryGetValue(config.Symbol, out convert))
                                //{
                                //    convert = await UnitConvertAsync(config.Symbol);
                                //    StaticObject.ContractToCurrencies.TryAdd(config.Symbol, convert);
                                //}

                                //var placedOrder = await api.OrderBookTrading.Trade.PlaceOrderAsync
                                //(
                                //    config.Symbol,
                                //    OkxTradeMode.Cross,
                                //    ordSide,
                                //    data.PositionSide ?? OkxPositionSide.Long,
                                //    OkxOrderType.LimitOrder,
                                //    data.FillQuantity ?? 0,
                                //    orderPrice2,
                                //    reduceOnly: true
                                //);

                                //var order = new OrderDto
                                //{
                                //    ExternalOrderId = placedOrder.Data.OrderId.ToString(),
                                //    Symbol = config.Symbol,
                                //    CandleStick = config.CandleStick,
                                //    Amount = config.Amount,
                                //    PositionSide = config.PositionSide,
                                //    Status = (int)OrderStatusEnums.Completed,
                                //    UserId = config.UserId,
                                //    CreatedDate = data.CreateTime,
                                //    TpPrice = orderPrice2,
                                //    Quantity = data.FillQuantity
                                //};
                                //StaticObject.FilledOrders.Add(order);
                            }
                        }
                        //if (orderState != OkxOrderState.Live)
                        //{
                        //    string orderIdStr = data?.OrderId?.ToString() ?? string.Empty;
                        //    var existingOrd = StaticObject.FilledOrders.FirstOrDefault(x => x.ExternalOrderId == orderIdStr);
                        //    if (existingOrd != null)
                        //    {
                        //        StaticObject.FilledOrders.TryTake(out existingOrd);
                        //    }
                        //}

                    });
                    StaticObject.OrderSubscriptions.TryAdd(user.Id + config.Symbol, result.Data);
                };
            }
            var subsToUnsubs = StaticObject.OrderSubscriptions.Where(o => !activeConfigs.Any(a => user.Id + a.Symbol == o.Key)).ToList();
            foreach (var unsub in subsToUnsubs)
            {
                await socket.UnsubscribeAsync(unsub.Value);
                StaticObject.OrderSubscriptions.TryRemove(unsub);
            }
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
            var orderPriceAndQuantity = CalculateOrderPriceQuantityTP(currentPrice, config);
            await api.V5Api.Trading.EditOrderAsync
                (
                    Category.Spot,
                    config.Symbol,
                    null,
                    config.OrderId,
                    null,
                    orderPriceAndQuantity.Item2,
                    orderPriceAndQuantity.Item1
                );
            return true;
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
                    clientOrderId: config.OrderId
                );

            if (cancelOrder != null && cancelOrder.Success)
            {
                _configService.DeleteConfig(config.Id);
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
            var orderSide = config.PositionSide == Constants.ShortSide ? OrderSide.Buy : OrderSide.Sell;
            if(config.OrderType == (int)OrderTypeEnums.Spot)
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
                    config.OrderType == (int) OrderTypeEnums.Margin,
                    clientOrderId: config.OrderId
                );
            StaticObject.FilledOrders.Add(config);
        }
        
        return true;
    }
}