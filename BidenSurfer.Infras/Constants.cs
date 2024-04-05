namespace BidenSurfer.Infras
{
    public class AppConstants
    {
        public const string PrivateKey = "6000ece7aa09440eb66ca8590d494021";
        public const string RedisAllBots = "RedisAllBots";
        public const string RedisAllUserBots = "RedisAllUserBots_{0}";
        public const string RedisAllUsers = "RedisAllUsers";
        public const string RedisConfigById = "RedisConfigById_{0}";
        public const string RedisAllConfigs = "RedisAllConfigs";
        public const string RedisAllScanners = "RedisAllScanners";
        public const string RedisAllOrders = "RedisAllOrders";
        public const string RedisFilledOrders = "RedisFilledOrders";
        public const string RedisOrdersByUser = "RedisOrdersByUser_{0}";
        public const string LongSide = "long";
        public const string ShortSide = "short";
        public const string CreatedByScanner = "scanner";
        public const string CreatedByUser = "user";
        public const string WebSocketAddress = "wss://ws.okx.com:8443/ws/v5/business";
    }
}