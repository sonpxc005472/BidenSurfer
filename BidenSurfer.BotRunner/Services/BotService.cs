namespace BidenSurfer.BotRunner.Services;

using BidenSurfer.Infras;
using BidenSurfer.Infras.Models;
using System.Collections.Generic;
using Bybit.Net.Clients;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using Bybit.Net.Enums;
using Bybit.Net.Objects.Models.V5;
using MassTransit;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Helpers;
using System;
using CryptoExchange.Net.CommonObjects;
using Microsoft.Extensions.Logging;
using Serilog;
using StackExchange.Redis;
using BidenSurfer.Infras.Entities;

public interface IBotService
{
    Task<BybitSocketClient> SubscribeSticker();
    Task<bool> UnsubscribeAll();
    Task SubscribeKline1m();
    Task InitUserApis();
    Task GetSpotSymbols();
    Task SubscribeOrderChannel();
    Task<bool> TakePlaceOrder(ConfigDto config, decimal currentPrice, bool isRetry = false, decimal previousAmount = 0);
    Task<bool> AmendOrder(ConfigDto config, decimal currentPrice, decimal openPrice);
    Task<bool> CancelOrder(ConfigDto config, bool isExpired);
    Task<bool> CancelAllOrder(long? userId = null);
}

public class BotService : IBotService
{
    private readonly IConfigService _configService;
    private readonly IUserService _userService;
    private readonly IBus _bus;
    private readonly ITeleMessage _teleMessage;
    private readonly ILogger<BotService> _logger;
    private SemaphoreSlim _mutex = new SemaphoreSlim(1);


    public BotService(IConfigService configService, IUserService userService, IBus bus, ITeleMessage teleMessage, ILogger<BotService> logger)
    {
        _configService = configService;
        _userService = userService;
        _bus = bus;
        _teleMessage = teleMessage;
        _logger = logger;
    }

    public async Task<BybitSocketClient> SubscribeSticker()
    {
        try
        {
            _logger.LogInformation("Subscribe ticker...");
            var configList = StaticObject.AllConfigs.Values.ToList();

            var symbols = configList.Where(c => c.IsActive).Select(c => c.Symbol).Distinct().ToList();

            foreach (var symbol in symbols)
            {
                if (!StaticObject.TickerSubscriptions.TryGetValue(symbol, out _))
                {
                    DateTime preTime = DateTime.Now;
                    DateTime preTimeCancel = DateTime.Now;
                    decimal prePrice = 0;
                    bool isBeeingTakeProfit = false;
                    var result = await StaticObject.PublicWebsocket.V5SpotApi.SubscribeToKlineUpdatesAsync(symbol, KlineInterval.OneMinute, async data =>
                    {
                        if (data != null)
                        {
                            var currentData = data.Data.FirstOrDefault();
                            var currentTime = DateTime.Now;
                            var currentPrice = currentData.ClosePrice;

                            // every 3s change order
                            if ((currentTime - preTime).TotalMilliseconds >= 3000)
                            {
                                preTime = currentTime;
                                var allActiveConfigs = StaticObject.AllConfigs;
                                var symbolConfigs = allActiveConfigs.Where(c => c.Value.Symbol == symbol && c.Value.IsActive).Select(c => c.Value).ToList();
                                var openScanners = symbolConfigs.Where(x => x.CreatedBy == AppConstants.CreatedByScanner && !string.IsNullOrEmpty(x.ClientOrderId)).ToList();
                                var editOrders = new Dictionary<long, List<BybitEditOrderRequest>>();
                                if (prePrice == 0)
                                {
                                    prePrice = currentPrice;
                                }
                                var priceDiff = Math.Abs(currentPrice - prePrice) / prePrice * 100;
                                bool isPriceChanged = false;
                                foreach (var symbolConfig in symbolConfigs)
                                {
                                    //Bot is stopping so do not do anymore
                                    if (StaticObject.BotStatus.ContainsKey(symbolConfig.UserId) && !StaticObject.BotStatus[symbolConfig.UserId])
                                    {
                                        continue;
                                    }
                                    bool isExistedScanner = openScanners.Any(x => x.UserId == symbolConfig.UserId);
                                    bool isLongSide = symbolConfig.PositionSide == AppConstants.LongSide;
                                    var existingFilledOrders = StaticObject.FilledOrders.Where(x => x.Value.UserId == symbolConfig.UserId && x.Value.OrderStatus == 2 && x.Value.Symbol == symbol).Select(r => r.Value).ToList();
                                    var sideOrderExisted = symbolConfigs.Any(x => x.UserId == symbolConfig.UserId && x.PositionSide != symbolConfig.PositionSide);
                                    if ((symbolConfig.CreatedBy != AppConstants.CreatedByScanner || (symbolConfig.CreatedBy == AppConstants.CreatedByScanner && !isExistedScanner)) && !existingFilledOrders.Any() && !sideOrderExisted && string.IsNullOrEmpty(symbolConfig.ClientOrderId))
                                    {
                                        //Place order
                                        await TakePlaceOrder(symbolConfig, currentPrice);
                                    }
                                    else if (!string.IsNullOrEmpty(symbolConfig.ClientOrderId) && !existingFilledOrders.Any())
                                    {
                                        //Nếu giá dịch chuyển lớn hơn 0.05% so với giá lúc trước thì amend order
                                        if (priceDiff > (decimal)0.05)
                                        {
                                            isPriceChanged = true;
                                            if(!symbolConfig.Timeout.HasValue || (symbolConfig.Timeout.HasValue && (currentTime - symbolConfig.Timeout.Value).TotalMilliseconds > 30000))
                                            {
                                                //Amend order
                                                await AmendOrder(symbolConfig, currentPrice, currentData.OpenPrice);
                                                var delayTime = NumberHelpers.RandomInt(100, 250);
                                                await Task.Delay(delayTime);
                                            }
                                        }
                                    }
                                }
                                if (isPriceChanged)
                                {
                                    prePrice = currentPrice;
                                }
                            }

                            await _mutex.WaitAsync();
                            try
                            {
                                //Đóng vị thế giá hiện tại nếu mở quá 3s mà chưa đóng được lần đầu, những lần sau sẽ đóng liên tục
                                var filledOrders = StaticObject.FilledOrders.Where(r => r.Value.Symbol == symbol).Select(r => r.Value).ToList();
                                foreach (var order in filledOrders)
                                {
                                    if (!order.isClosingFilledOrder)
                                    {
                                        if ((currentTime - order.EditedDate.Value).TotalMilliseconds > 3500)
                                        {
                                            await TryTakeProfit(order, currentPrice);
                                        }
                                    }
                                    else
                                    {
                                        if ((currentTime - order.EditedDate.Value).TotalMilliseconds > 500)
                                        {
                                            await TryTakeProfit(order, currentPrice);
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
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Subscribe ticker error: " + ex.Message);
        }
        return StaticObject.PublicWebsocket;
    }

    public async Task<bool> TakePlaceOrder(ConfigDto? config, decimal currentPrice, bool isRetry = false, decimal previosAmount = 0)
    {
        var userSetting = StaticObject.AllUsers.FirstOrDefault(u => u.Id == config.UserId)?.Setting;
        try
        {
            if (StaticObject.FilledOrders.ContainsKey(config.CustomId))
            {
                _logger.LogInformation($"Take order {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange}: having another order to complete");
                return false;
            }
            
            if (userSetting == null) return false;
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
                var orderPriceAndQuantity = CalculateOrderPriceQuantityTP(currentPrice, config, isRetry, previosAmount);
                var orderSide = config.PositionSide == AppConstants.ShortSide ? OrderSide.Sell : OrderSide.Buy;
                if (config.OrderType == (int)OrderTypeEnums.Spot)
                {
                    orderSide = OrderSide.Buy;
                }
                string clientOrderId = Guid.NewGuid().ToString();
                config.ClientOrderId = clientOrderId;
                config.TPPrice = orderPriceAndQuantity.Item3;
                config.OrderStatus = 1;
                config.TotalQuantity = orderPriceAndQuantity.Item2;
                config.Timeout = null;
                _configService.AddOrEditConfig(config);
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
                    _logger.LogInformation($"Took order {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange}: {clientOrderId}");
                    config.OrderId = placedOrder.Data.OrderId;
                    _configService.AddOrEditConfig(config);
                }
                else
                {
                    if ((placedOrder.Error?.Code == 131212 || placedOrder.Error?.Code == 170131) && !isRetry && previosAmount > 0)
                    {
                        await TakePlaceOrder(config, currentPrice, true, previosAmount);
                    }
                    else
                    {                       
                        var message = $"Take order {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} error: {placedOrder?.Error?.Message} - code: {placedOrder?.Error?.Code}";
                        _logger.LogInformation(message);
                       
                        await TakeOrderError(config, placedOrder?.Error?.Message ?? string.Empty, userSetting.TeleChannel);
                    }

                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // log error to the telegram channels
            _logger.LogInformation($"Take order {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} Ex: {ex.Message}");
            await TakeOrderError(config, ex.Message, userSetting.TeleChannel);
            return false;
        }
    }

    private async Task TakeOrderError(ConfigDto config, string message, string teleChannel)
    {
        config.IsActive = false;
        _configService.AddOrEditConfig(config);
        _ = _teleMessage.ErrorMessage(config.Symbol, config.OrderChange.ToString(), config.PositionSide, teleChannel, message ?? string.Empty);

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

    public async Task<bool> AmendOrder(ConfigDto config, decimal currentPrice, decimal openPrice)
    {
        var userSetting = StaticObject.AllUsers.FirstOrDefault(u => u.Id == config.UserId)?.Setting;
        if (userSetting == null) return false;
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
            if ((config.PositionSide == AppConstants.ShortSide && orderPrice <= openPrice) || (config.PositionSide == AppConstants.LongSide && orderPrice >= openPrice))
            {
                orderPrice = openPrice;
                var instrumentDetail = StaticObject.Symbols.FirstOrDefault(i => i.Name == config.Symbol);
                var tpPrice = config.PositionSide == AppConstants.ShortSide ? orderPrice - ((currentPrice * config.OrderChange / 100) * _tp / 100) : orderPrice + ((currentPrice * config.OrderChange / 100) * _tp / 100);
                tpPriceUpdate = (decimal)(((int)(tpPrice / instrumentDetail?.PriceFilter?.TickSize ?? 1)) * instrumentDetail?.PriceFilter?.TickSize);
            }
            var amendOrder = await api.V5Api.Trading.EditOrderAsync
                (
                    Category.Spot,
                    symbol,
                    clientOrderId: config.ClientOrderId,
                    quantity: orderPriceAndQuantity.Item2,
                    price: orderPrice
                );
            if (amendOrder.Success)
            {
                config.TPPrice = tpPriceUpdate;
                config.OrderStatus = 1;
                config.Timeout = null;
                config.TotalQuantity = orderPriceAndQuantity.Item2;
                _configService.AddOrEditConfig(config);
                return true;
            }
            else
            {
                if (amendOrder.Error.Message.Contains("Request timed out", StringComparison.InvariantCultureIgnoreCase) || amendOrder.Error.Message.Contains("Timestamp for this request is outside of the recvWindow", StringComparison.InvariantCultureIgnoreCase))
                { 
                    config.Timeout = DateTime.Now;
                    _ = _teleMessage.ErrorMessage(config.Symbol, config.OrderChange.ToString(), config.PositionSide, userSetting.TeleChannel, $"Amend Error: {amendOrder.Error.Message}");
                }
                else if (IsNeededCancel(amendOrder.Error.Message))
                {
                    var message = $"Amend {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} error: {amendOrder.Error?.Code} - {amendOrder.Error?.Message}";
                    _logger.LogInformation(message);
                    _ = _teleMessage.ErrorMessage(config.Symbol, config.OrderChange.ToString(), config.PositionSide, userSetting.TeleChannel, $"Amend Error: {amendOrder.Error.Message}");
                    await CancelOrder(config);
                }

            }
            return false;
        }
        catch (Exception ex)
        {
            var message = $"Amend {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} Ex: {ex.Message}";
            _logger.LogInformation(message);
            _ = _teleMessage.ErrorMessage(config.Symbol, config.OrderChange.ToString(), config.PositionSide, userSetting.TeleChannel, $"Amend Error: {ex.Message}");
            await CancelOrder(config);
            return false;
        }
    }

    public async Task<bool> CancelOrder(ConfigDto config, bool isExpired = false)
    {
        try
        {
            var userSetting = StaticObject.AllUsers.FirstOrDefault(u => u.Id == config.UserId)?.Setting;
            if (userSetting == null) return false;
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
                StaticObject.IsInternalCancel = true;
                var cancelOrder = await api.V5Api.Trading.CancelOrderAsync
                    (
                        Category.Spot,
                        config.Symbol,
                        clientOrderId: config.ClientOrderId
                    );
                config.OrderStatus = null;
                config.ClientOrderId = string.Empty;
                config.OrderId = string.Empty;
                config.isClosingFilledOrder = false;
                config.IsActive = false;
                config.EditedDate = DateTime.Now;
                config.Amount = config.OriginAmount.HasValue ? config.OriginAmount.Value : config.Amount;
                _configService.AddOrEditConfig(config);

                if (cancelOrder.Success)
                {
                    var messageSub = isExpired ? $"Expired {config.Expire}m" : $"Cancelled";
                    var message = isExpired ? $"{config.Symbol} | {config.PositionSide.ToUpper()}| {config.OrderChange.ToString()} {messageSub}" : $"{config.Symbol} | {config.PositionSide.ToUpper()}| {config.OrderChange.ToString()} {messageSub}";
                    _logger.LogInformation(message);
                    _ = _teleMessage.OffConfigMessage(config.Symbol, config.OrderChange.ToString(), config.PositionSide, userSetting.TeleChannel, messageSub);
                }
                else
                {
                    _logger.LogInformation($"{DateTime.Now} - Cancel order {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} error: {cancelOrder.Error.Message}");
                }
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
                await Task.Delay(200);
                StaticObject.IsInternalCancel = false;
            }
        }
        catch (Exception ex)
        {
            // log error to the telegram channels
            _logger.LogInformation($"{DateTime.Now} - Cancel order {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} Ex: {ex.Message}");
            StaticObject.IsInternalCancel = false;
            return false;
        }
        return true;
    }

    public async Task SubscribeKline1m()
    {
        try
        {
            bool isNotified = false;
            var preTimeCancel = DateTime.Now;

            var result = await StaticObject.PublicWebsocket.V5SpotApi.SubscribeToKlineUpdatesAsync(new List<string> { "BTCUSDT" }, KlineInterval.OneMinute, async data =>
            {
                DateTime currentTime = DateTime.Now;
                TimeSpan timeSinceMidnight = currentTime.TimeOfDay;
                double hoursSinceMidnight = Math.Floor(timeSinceMidnight.TotalHours);

                // Cancel order if expired
                if ((currentTime - preTimeCancel).TotalMilliseconds >= 10000)
                {
                    preTimeCancel = currentTime;
                    var configExpired = StaticObject.AllConfigs.Where(x => (x.Value.IsActive && !string.IsNullOrEmpty(x.Value.OrderId) && x.Value.EditedDate != null && x.Value.Expire != null && x.Value.Expire.Value != 0 && x.Value.EditedDate.Value.AddMinutes(x.Value.Expire.Value) < currentTime)).Select(c => c.Value).ToList();
                    var cancelledConfigs = new List<string>();
                    foreach (var config in configExpired)
                    {
                        var isFilledOrder = StaticObject.FilledOrders.Any(x => x.Key == config.CustomId);
                        if (!isFilledOrder)
                        {
                            config.IsActive = false;
                            await CancelOrder(config, true);
                        }
                    }

                    //Amount increase Expired
                    var configAmountExpired = StaticObject.AllConfigs
                    .Where(x => x.Value.IsActive
                            && !string.IsNullOrEmpty(x.Value.OrderId)
                            && x.Value.OrderStatus != 2
                            && x.Value.EditedDate != null
                            && x.Value.IncreaseAmountExpire != null
                            && x.Value.IncreaseAmountExpire.Value > 0
                            && x.Value.EditedDate.Value.AddMinutes(x.Value.IncreaseAmountExpire.Value) < currentTime
                            && x.Value.Amount != x.Value.OriginAmount)
                    .Select(c => c.Value).ToList();
                    if (configAmountExpired.Any())
                    {
                        foreach (var config in configAmountExpired)
                        {
                            _logger.LogInformation($"Amount increase Expired: {config.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange} - Current: {config.Amount}, Origin: {config.OriginAmount }");
                            config.Amount = config.OriginAmount.HasValue ? config.OriginAmount.Value : config.Amount;
                            _configService.AddOrEditConfig(config);
                        }
                        _ = _bus.Send(new AmountExpireMessage { Configs = configAmountExpired.Select(c => c.CustomId).ToList() });
                    }
                }

                // Notify wallet every 3 hours
                if (hoursSinceMidnight % 3 == 0 && !isNotified)
                {
                    isNotified = true;
                    var users = StaticObject.AllUsers.Where(u => u.Status == (int)UserStatusEnums.Active && u.Setting != null).ToList();
                    foreach (var user in users)
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

                        var wallet = await api.V5Api.Account.GetBalancesAsync(AccountType.Unified);
                        if (wallet.Success && wallet.Data != null)
                        {
                            var balance = Math.Round(wallet.Data.List.FirstOrDefault()?.TotalWalletBalance ?? 0, 0);
                            var budget = Math.Round((await _userService.GetGeneralSetting(user.Id))?.Budget ?? 0, 0);
                            var pnlCash = Math.Round(balance - budget, 0);
                            var pnlPercent = budget > 0 ? Math.Round((pnlCash / budget) * 100, 2) : 0;
                            _ = _teleMessage.WalletNotifyMessage(balance, budget, pnlCash, pnlPercent, user.Setting.TeleChannel);
                        }
                    }
                }
                else if (hoursSinceMidnight % 3 != 0)
                {
                    isNotified = false;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Subscribe kline error: " + ex.Message);
        }
    }

    public async Task SubscribeOrderChannel()
    {
        try
        {
            _logger.LogInformation("Subscribe order channel...");
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
                        //_logger.LogInformation($"Order updated: data: {JsonSerializer.Serialize(data.Data)} - original: {JsonSerializer.Serialize(data.OriginalData)}");
                        var updatedDatas = data.Data.ToList();
                        foreach (var updatedData in updatedDatas)
                        {
                            var orderState = updatedData?.Status;
                            var orderId = updatedData?.OrderId;
                            var clientOrderId = updatedData?.ClientOrderId;
                            if (orderState != Bybit.Net.Enums.V5.OrderStatus.New && orderState != Bybit.Net.Enums.V5.OrderStatus.Created)
                            {
                                _logger.LogInformation($"{updatedData?.Symbol} | {orderState} | {clientOrderId}");                               
                            }
                            var config = StaticObject.AllConfigs.FirstOrDefault(c => c.Value.ClientOrderId == clientOrderId).Value;
                            if (config == null)
                            {
                                config = StaticObject.FilledOrders.FirstOrDefault(c => c.Value.ClientOrderId == clientOrderId && c.Value.OrderStatus == 2).Value;
                            }

                            if (config == null)
                            {
                                _logger.LogInformation("Null order: " + clientOrderId);
                                if (orderState == Bybit.Net.Enums.V5.OrderStatus.PartiallyFilled)
                                {
                                    _ = _teleMessage.FillMessage(updatedData.Symbol, "", "", user.Setting?.TeleChannel, false, updatedData.QuantityFilled ?? 0, 0, updatedData.AveragePrice ?? 0);
                                }
                                else if (orderState == Bybit.Net.Enums.V5.OrderStatus.Filled)
                                {
                                    _ = _teleMessage.FillMessage(updatedData.Symbol, "", "", user.Setting?.TeleChannel, true, updatedData.QuantityFilled ?? 0, 0, updatedData.AveragePrice ?? 0);
                                }
                                continue;
                            }
                            if (orderState == Bybit.Net.Enums.V5.OrderStatus.PartiallyFilled)
                            {
                                var closingOrder = StaticObject.FilledOrders.Any(c => c.Value.ClientOrderId == clientOrderId && c.Value.OrderStatus == 2);
                                if (!closingOrder)
                                {
                                    _logger.LogInformation($"{updatedData?.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange.ToString()} - PartiallyFilled - {clientOrderId}");

                                    _ = _teleMessage.FillMessage(updatedData.Symbol, config.OrderChange.ToString(), config.PositionSide, user.Setting.TeleChannel, false, updatedData.QuantityFilled ?? 0, config.TotalQuantity ?? 0, updatedData.AveragePrice ?? 0);
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
                                        clientOrderId: clientOrderId
                                    );
                                    if (cancelOrder.Success)
                                    {
                                        await TakeProfit(updatedData, user);
                                    }
                                }

                            }
                            else if (orderState == Bybit.Net.Enums.V5.OrderStatus.Filled)
                            {
                                var closingOrder = StaticObject.FilledOrders.FirstOrDefault(c => c.Value.ClientOrderId == clientOrderId && c.Value.OrderStatus == 2).Value;
                                if (closingOrder == null)
                                {
                                    await TakeProfit(updatedData, user);
                                    _logger.LogInformation($"{updatedData?.Symbol} | {config.PositionSide.ToUpper()} | {config.OrderChange}| {clientOrderId} - Filled");
                                    _ = _teleMessage.FillMessage(updatedData.Symbol, config.OrderChange.ToString(), config.PositionSide, user.Setting.TeleChannel, true, updatedData.QuantityFilled ?? 0, config.TotalQuantity ?? 0, updatedData.AveragePrice ?? 0);
                                }
                                else
                                {
                                    await _mutex.WaitAsync();
                                    try
                                    {
                                        var totalFee = closingOrder.TotalFee ?? 0;
                                        var currentFee = (updatedData?.ExecutedFee ?? 0) * (updatedData?.AveragePrice ?? 0);
                                        var isRemoved = StaticObject.FilledOrders.TryRemove(closingOrder.CustomId, out _);
                                        _logger.LogInformation($"Take profit: {updatedData.Symbol} - removed: {isRemoved}");

                                        var openPrice = closingOrder.FilledPrice ?? 0;
                                        var closePrice = updatedData?.AveragePrice ?? 0;
                                        var filledQuantity = updatedData?.QuantityFilled ?? 0;
                                        var tp = closingOrder.PositionSide == AppConstants.ShortSide ? (openPrice - closePrice) * filledQuantity : (closePrice - openPrice) * filledQuantity;
                                        var pnlCash = Math.Round(tp - (totalFee + currentFee), 2);
                                        var pnlPercent = Math.Round((tp / (openPrice * filledQuantity)) * 100, 2);
                                        var pnlText = pnlCash > 0 ? "WIN" : "LOSE";
                                        _logger.LogInformation($"{updatedData?.Symbol}|{closingOrder.OrderChange}|{pnlText}|PNL: ${pnlCash.ToString("0.00")} {pnlPercent.ToString("0.00")}%");
                                        var configWin = _configService.UpsertWinLose(config, pnlCash > 0);
                                        _ = _teleMessage.PnlMessage(updatedData.Symbol, config.OrderChange.ToString(), config.PositionSide, user.Setting.TeleChannel, pnlCash > 0, pnlCash, pnlPercent, configWin.Win, configWin.Total, filledQuantity, config.TotalQuantity ?? 0, openPrice, closePrice);
                                        config.OrderId = string.Empty;
                                        config.ClientOrderId = string.Empty;
                                        config.OrderStatus = null;
                                        config.IsActive = !(config.CreatedBy == AppConstants.CreatedByScanner && pnlCash <= 0) || pnlCash > 0;
                                        config.isClosingFilledOrder = false;
                                        config.EditedDate = DateTime.Now;
                                        var amountIncrease = config.Amount;
                                        var beforeIncrease = config.Amount;
                                        bool isChanged = false;

                                        if (pnlCash <= 0 && config.CreatedBy != AppConstants.CreatedByScanner && config.IncreaseOcPercent != null && config.IncreaseOcPercent > 0)
                                        {
                                            isChanged = true;
                                            config.OrderChange = config.OrderChange + (config.OrderChange * config.IncreaseOcPercent.Value / 100);
                                        }
                                        if (config.IncreaseAmountPercent != null && config.IncreaseAmountPercent > 0)
                                        {
                                            isChanged = true;
                                            amountIncrease = config.Amount + (config.Amount * config.IncreaseAmountPercent.Value / 100);
                                            if (config.AmountLimit != null && config.AmountLimit > 0)
                                            {
                                                amountIncrease = amountIncrease > config.AmountLimit ? config.AmountLimit.Value : amountIncrease;
                                            }
                                        }

                                        config.Amount = config.CreatedBy == AppConstants.CreatedByScanner && pnlCash <= 0 ? (config.OriginAmount.HasValue ? config.OriginAmount.Value : config.Amount) : amountIncrease;

                                        if (pnlCash <= 0 && config.CreatedBy == AppConstants.CreatedByScanner)
                                        {
                                            config.IsActive = false;
                                            _configService.AddOrEditConfig(config);

                                            await _bus.Send(new OffConfigMessage
                                            {
                                                Configs = new List<string>
                                            {
                                                config.CustomId
                                            }
                                            });
                                            await _bus.Send(new OnOffConfigMessageScanner()
                                            {
                                                Configs = new List<ConfigDto>
                                            {
                                                new ConfigDto
                                                {
                                                    CustomId = config.CustomId,
                                                    IsActive = false,
                                                }
                                            }
                                            });
                                        }
                                        else
                                        {
                                            await TakePlaceOrder(config, closePrice, false, beforeIncrease == amountIncrease ? 0 : beforeIncrease);
                                            if (isChanged)
                                            {
                                                await _bus.Send(new UpdateConfigMessage()
                                                {
                                                    Configs = new List<ConfigDto>
                                                {
                                                    config
                                                }
                                                });
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        _mutex.Release();
                                    }
                                }
                            }
                            else if (orderState == Bybit.Net.Enums.V5.OrderStatus.Cancelled && !StaticObject.IsInternalCancel)
                            {
                                var customId = config.CustomId;
                                config.IsActive = false;
                                _configService.UpsertConfigs(new List<ConfigDto> { config });

                                await _bus.Send(new OffConfigMessage
                                {
                                    Configs = new List<string>
                                    {
                                        customId
                                    }
                                });
                                await _bus.Send(new OnOffConfigMessageScanner
                                {
                                    Configs = new List<ConfigDto>
                                    {
                                        new ConfigDto
                                        {
                                            CustomId = customId,
                                            IsActive = false,
                                        }
                                    }
                                });
                                _ = _teleMessage.OffConfigMessage(updatedData.Symbol, config.OrderChange.ToString(), config.PositionSide, user.Setting.TeleChannel, "Cancelled");
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
        catch (Exception ex)
        {
            _logger.LogInformation("Subscribe order channel error: " + ex.Message);
        }
    }

    public async Task InitUserApis()
    {
        await GetSpotSymbols();

        foreach (var user in StaticObject.AllUsers)
        {
            if (user.Status == (int)UserStatusEnums.Active && !string.IsNullOrEmpty(user.Setting?.ApiKey) && !string.IsNullOrEmpty(user.Setting.SecretKey))
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
                var marginSymbols = StaticObject.Symbols.Where(c => (c.MarginTrading == MarginTrading.Both || c.MarginTrading == MarginTrading.UtaOnly) && c.Name.EndsWith("USDT")).Select(c => new { c.Name, c.BaseAsset }).Distinct().ToList();
                var symbolCollateral = _userService.GetSymbolCollateral(user.Id);
                var newCollaterals = new List<string>();
                foreach (var symbol in marginSymbols)
                {
                    if (!symbolCollateral.Any(c => c == symbol.BaseAsset))
                    {
                        var rs = await api.V5Api.Account.SetCollateralAssetAsync(symbol.BaseAsset, true);
                        await api.V5Api.Account.SetLeverageAsync(Category.Spot, symbol.Name, 10, 10);
                        newCollaterals.Add(symbol.BaseAsset);
                    }
                }
                if (newCollaterals.Any())
                {
                    _userService.SaveSymbolCollateral(user.Id, newCollaterals);
                }
            }
        }

    }

    private async Task TakeProfit(BybitOrderUpdate orderUpdate, UserDto user, int tryCount = 0, decimal quantity = 0)
    {
        var configToUpdate = StaticObject.AllConfigs.FirstOrDefault(c => c.Value.ClientOrderId == orderUpdate?.ClientOrderId).Value;
        if (configToUpdate == null)
        {
            _logger.LogInformation($"Take Profit {orderUpdate.Symbol}|{orderUpdate.Side}|{configToUpdate.OrderChange}| {orderUpdate?.ClientOrderId} error: Config not found");
            return;
        }
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
            quantity = tryCount == 0 ? (orderUpdate?.QuantityFilled ?? 0) : quantity * 0.9989M;
            var orderPrice = CalculateTP(orderUpdate.AveragePrice, configToUpdate);
            var instrumentDetail = StaticObject.Symbols.FirstOrDefault(i => i.Name == orderUpdate.Symbol);
            var quantityWithTicksize = ((long)(quantity / instrumentDetail?.LotSizeFilter?.BasePrecision ?? 1)) * instrumentDetail?.LotSizeFilter?.BasePrecision;

            var clientOrderId = Guid.NewGuid().ToString();
            _logger.LogInformation($"Taking profit {orderUpdate.Symbol}|{ordSide}|quantity: {quantity}|{clientOrderId} - count: {tryCount}");
            var cloneConfig = configToUpdate.Clone();
            cloneConfig.ClientOrderId = clientOrderId;
            cloneConfig.FilledPrice = orderUpdate.AveragePrice;
            cloneConfig.FilledQuantity = quantityWithTicksize;
            cloneConfig.OrderStatus = 2;
            cloneConfig.EditedDate = DateTime.Now;
            cloneConfig.TotalFee = (orderUpdate?.ExecutedFee ?? 0) * (orderUpdate?.AveragePrice ?? 0);
            StaticObject.FilledOrders.TryAdd(cloneConfig.CustomId, cloneConfig);

            var placedOrder = await api.V5Api.Trading.PlaceOrderAsync
            (
                Category.Spot,
                orderUpdate?.Symbol,
                ordSide,
                NewOrderType.Limit,
                quantityWithTicksize ?? 0,
                orderPrice,
                false,
                clientOrderId: clientOrderId
            );

            if (!placedOrder.Success)
            {
                StaticObject.FilledOrders.TryRemove(cloneConfig.CustomId, out _);
                if (tryCount <= 3)
                {
                    _logger.LogInformation($"Retry take profit {orderUpdate.Symbol}|{orderUpdate.Side}|{configToUpdate.OrderChange}| ${quantity * orderPrice} | try: {tryCount + 1} error: {placedOrder?.Error?.Message} - code: {placedOrder?.Error?.Code}");
                    await TakeProfit(orderUpdate, user, tryCount + 1, quantity);
                }
                else
                {
                    _logger.LogInformation($"Take Profit {orderUpdate.Symbol}|{orderUpdate.Side}|{configToUpdate.OrderChange}| ${(orderUpdate?.QuantityFilled ?? 0) * orderPrice} error: {placedOrder?.Error?.Message} - code: {placedOrder?.Error?.Code}");
                }
            }

        }
        catch (Exception ex)
        {
            StaticObject.FilledOrders.TryRemove(configToUpdate.CustomId, out _);
            _logger.LogInformation($"Take Profit {orderUpdate.Symbol}|{orderUpdate.Side}|{configToUpdate.OrderChange} Ex: {ex.Message}");
        }

    }

    private async Task<bool> TryTakeProfit(ConfigDto config, decimal currentPrice)
    {
        try
        {
            var userSetting = StaticObject.AllUsers.FirstOrDefault(u => u.Id == config.UserId)?.Setting;
            if (userSetting == null) return false;
            BybitRestClient api;
            if (!StaticObject.RestApis.TryGetValue(config.UserId, out api))
            {
                api = new BybitRestClient();
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
                    clientOrderId: config.ClientOrderId,
                    quantity: config.FilledQuantity,
                    price: orderPriceWithTicksize
                );

            config.EditedDate = DateTime.Now;

            if (!amendOrder.Success)
            {
                _logger.LogInformation($"Try to take profit {config.Symbol}|{config.PositionSide}|{config.OrderChange}|{config.ClientOrderId} Error: {amendOrder.Error?.Code}-{amendOrder.Error?.Message}");
                var isRemoved = StaticObject.FilledOrders.TryRemove(config.CustomId, out _);
                _logger.LogInformation($"Try to take profit: removed: {isRemoved} - {config.Symbol}|{config.PositionSide}|{config.OrderChange}|{config.ClientOrderId}");
                config.OrderId = string.Empty;
                config.ClientOrderId = string.Empty;
                config.OrderStatus = 1;
                config.isClosingFilledOrder = false;
            }
            else
            {
                config.isClosingFilledOrder = true;
                StaticObject.FilledOrders[config.CustomId] = config;
            }
            _configService.AddOrEditConfig(config);
            return amendOrder.Success;
        }
        catch (Exception ex)
        {
            // Log error to telegram
            _logger.LogInformation($"Try to take profit {config.Symbol}|{config.PositionSide}|{config.OrderChange} Ex: {ex.Message}");
            var isRemoved = StaticObject.FilledOrders.TryRemove(config.CustomId, out _);
            _logger.LogInformation($"Try to take profit: removed: {isRemoved} - {config.Symbol}|{config.PositionSide}|{config.OrderChange}|{config.ClientOrderId}");
            return false;
        }
    }


    private readonly decimal _tp = 80;
    private (decimal, decimal, decimal) CalculateOrderPriceQuantityTP(decimal currentPrice, ConfigDto config, bool isRetry = false, decimal previousAmount = 0)
    {
        var orderSide = config.PositionSide == AppConstants.ShortSide ? OrderSide.Sell : OrderSide.Buy;
        var orderPrice = config.PositionSide == AppConstants.ShortSide ? currentPrice + (currentPrice * config.OrderChange / 100) : currentPrice - (currentPrice * config.OrderChange / 100);
        var instrumentDetail = StaticObject.Symbols.FirstOrDefault(i => i.Name == config.Symbol);
        var orderPriceWithTicksize = ((int)(orderPrice / instrumentDetail?.PriceFilter?.TickSize ?? 1)) * instrumentDetail?.PriceFilter?.TickSize;
        var quantity = (isRetry ? previousAmount : config.Amount) / orderPriceWithTicksize;
        var quantityWithTicksize = ((long)(quantity / instrumentDetail?.LotSizeFilter?.BasePrecision ?? 1)) * instrumentDetail?.LotSizeFilter?.BasePrecision;
        var tpPrice = config.PositionSide == AppConstants.ShortSide ? orderPrice - ((currentPrice * config.OrderChange / 100) * _tp / 100) : orderPrice + ((currentPrice * config.OrderChange / 100) * _tp / 100);
        var tpPriceWithTicksize = ((int)(tpPrice / instrumentDetail?.PriceFilter?.TickSize ?? 1)) * instrumentDetail?.PriceFilter?.TickSize;


        return (orderPriceWithTicksize ?? 0, quantityWithTicksize ?? 0, tpPriceWithTicksize ?? 0);
    }

    private decimal? CalculateTP(decimal? filledPrice, ConfigDto config)
    {
        if (filledPrice == null) return 0;
        var instrumentDetail = StaticObject.Symbols.FirstOrDefault(i => i.Name == config.Symbol);
        var startPrice = config.PositionSide == AppConstants.ShortSide ? (100 * filledPrice / (100 + config.OrderChange)) : (100 * filledPrice / (100 - config.OrderChange));
        var tpPrice = config.PositionSide == AppConstants.ShortSide ? filledPrice - ((startPrice * config.OrderChange / 100) * _tp / 100) : filledPrice + ((startPrice * config.OrderChange / 100) * _tp / 100);
        var tpPriceWithTicksize = ((int)(tpPrice / instrumentDetail?.PriceFilter?.TickSize ?? 1)) * instrumentDetail?.PriceFilter?.TickSize;


        return tpPriceWithTicksize;
    }

    public async Task GetSpotSymbols()
    {
        try
        {
            var publicApi = new BybitRestClient();
            var spotSymbols = (await publicApi.V5Api.ExchangeData.GetSpotSymbolsAsync()).Data.List;
            var symbolInfos = spotSymbols.Where(c => (c.MarginTrading == MarginTrading.Both || c.MarginTrading == MarginTrading.UtaOnly) && c.Name.EndsWith("USDT")).ToList() ?? new List<BybitSpotSymbol>();

            StaticObject.Symbols = symbolInfos.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Get spot symbols error: " + ex.Message);
        }
    }

    public async Task<bool> CancelAllOrder(long? userId = null)
    {
        try
        {
            _logger.LogInformation("Cancel all orders...");
            StaticObject.IsInternalCancel = true;
            if (userId != null)
            {
                var userSetting = StaticObject.AllUsers.FirstOrDefault(u => u.Id == userId.Value)?.Setting;
                BybitRestClient api;
                if (!StaticObject.RestApis.TryGetValue(userId.Value, out api))
                {
                    api = new BybitRestClient();

                    if (userSetting != null)
                    {
                        api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey, ApiCredentialsType.Hmac));
                        StaticObject.RestApis.TryAdd(userId.Value, api);
                    }
                }

                if (api != null)
                {
                    var rs = await api.V5Api.Trading.CancelAllOrderAsync
                        (
                            Category.Spot
                        );
                    foreach (var order in StaticObject.AllConfigs.Where(o => o.Value.UserId == userId.Value).Select(r => r.Value).ToList())
                    {
                        order.OrderId = string.Empty;
                        order.ClientOrderId = string.Empty;
                        order.OrderStatus = null;
                        order.IsActive = false;
                        order.EditedDate = DateTime.Now;
                        _configService.AddOrEditConfig(order);
                    }
                }
            }
            else
            {
                foreach (var user in StaticObject.AllUsers)
                {
                    var userSetting = StaticObject.AllUsers.FirstOrDefault(u => u.Id == user.Id)?.Setting;
                    BybitRestClient api;
                    if (!StaticObject.RestApis.TryGetValue(user.Id, out api))
                    {
                        api = new BybitRestClient();

                        if (userSetting != null)
                        {
                            api.SetApiCredentials(new ApiCredentials(userSetting.ApiKey, userSetting.SecretKey, ApiCredentialsType.Hmac));
                            StaticObject.RestApis.TryAdd(user.Id, api);
                        }
                    }

                    if (api != null)
                    {
                        var rs = await api.V5Api.Trading.CancelAllOrderAsync
                            (
                                Category.Spot
                            );
                    }
                }
            }

            await Task.Delay(200);
            StaticObject.IsInternalCancel = false;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.Message);
            return false;
        }
    }

    private bool IsNeededCancel(string errorMessage)
    {
        return !errorMessage.Contains("not exist", StringComparison.InvariantCultureIgnoreCase) && !errorMessage.Contains("The order remains unchanged", StringComparison.InvariantCultureIgnoreCase) && !errorMessage.Contains("pending order modification", StringComparison.InvariantCultureIgnoreCase);
    }

    public async Task<bool> UnsubscribeAll()
    {
        _logger.LogInformation("Unsubscribe all...");

        var subsToUnsubs = StaticObject.TickerSubscriptions.ToList();
        foreach (var unsub in subsToUnsubs)
        {
            await StaticObject.PublicWebsocket.UnsubscribeAsync(unsub.Value);
        }
        StaticObject.TickerSubscriptions.Clear();
        return true;
    }
}