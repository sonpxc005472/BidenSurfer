namespace BidenSurfer.Infras
{
    public class DataSend
    {
        public string op { get; set; }
        public object args { get; set; }
    }

    public class DataResponse
    {
        public object arg { get; set; }
        public object data { get; set; }
    }

    public class LoginParam
    {
        public string? apiKey { get; set; }
        public string? reqTime { get; set; }
        public string? signature { get; set; }
    }

    public class CandleStickParam
    {
        public string? channel { get; set; }
        public string? instId { get; set; }
    }

    public class CandleStickPushResponse
    {
        public string? symbol { get; set; }
        public string? channel { get; set; }
        public long? ts { get; set; }
        public CandleStickPushDataResponse? data { get; set; }
    }

    public class UnitConvertGetResponse
    {
        public string? code { get; set; }
        public string? msg { get; set; }
        public List<UnitConvertDataGetResponse> data { get; set; }
    }

    public class UnitConvertDataGetResponse
    {
        public string? instId { get; set; }
        public string? px { get; set; }
        public string? sz { get; set; }
        public string? type { get; set; }
        public string? unit { get; set; }
    }

    public class CandleStickPushDataResponse
    {
        public double? a { get; set; }
        public double? c { get; set; }
        public double? l { get; set; }
        public double? o { get; set; }
        public double? q { get; set; }
        public double? h { get; set; }
        public string? interval { get; set; }
        public string? symbol { get; set; }
    }

    public class OrderPushResponse
    {
        public string? channel { get; set; }
        public long? ts { get; set; }
        public OrderPushDataResponse? data { get; set; }
    }

    public class OrderPushDataResponse
    {
        public int? category { get; set; }
        public long? createTime { get; set; }
        public double? dealAvgPrice { get; set; }
        public double? dealVol { get; set; }
        public int? errorCode { get; set; }
        public string? externalOid { get; set; }
        public string? feeCurrency { get; set; }
        public int? leverage { get; set; }
        public double? makerFee { get; set; }
        public int? openType { get; set; }
        public double? orderMargin { get; set; }
        public int? orderType { get; set; }
        public long? positionId { get; set; }
        public double? price { get; set; }
        public double? remainVol { get; set; }
        public double? profit { get; set; }
        public int? side { get; set; }
        public int? state { get; set; }
        public string? symbol { get; set; }
        public double? takerFee { get; set; }
        public long? updateTime { get; set; }
        public double? usedMargin { get; set; }
        public double? version { get; set; }
        public double? vol { get; set; }
    }
}
