namespace BidenSurfer.Infras
{
    public class AppConstants
    {
        public const string PrivateKey = "6000ece7aa09440eb66ca8590d494021";
        public const string RedisAllBots = "RedisAllBots";
        public const string RedisAllUserBots = "RedisAllUserBots_{0}";
        public const string RedisAllUsers = "RedisAllUsers";
        public const string RedisSymbolCollateral = "RedisSymbolCollateral";
        public const string RedisConfigById = "RedisConfigById_{0}";
        public const string RedisAllConfigs = "RedisAllConfigs";
        public const string RedisAllScanners = "RedisAllScanners";
        public const string RedisAllScannerSetting = "RedisAllScannerSetting";
        public const string RedisAllOrders = "RedisAllOrders";
        public const string RedisFilledOrders = "RedisFilledOrders";
        public const string RedisOrdersByUser = "RedisOrdersByUser_{0}";
        public const string RedisConfigWinLose = "RedisConfigWinLose";
        public const string LongSide = "long";
        public const string ShortSide = "short";
        public const string CreatedByScanner = "scanner";
        public const string CreatedByUser = "user";
        public const string USER_CLAIM_TYPE = "userId";
        public const string ROLE_CLAIM_TYPE = "role";

        /* Error Messages */

        public const string OrderNotExist = "Order does not exist";
        public const string PendingOrderModification = "pending order modification";
        public const string OrderRemainsUnchanged = "The order remains unchanged";
        public const string RequestTimeout = "Request timed out";
        public const string RequestOutsideRecvWindow = "Timestamp for this request is outside of the recvWindow";
    }
}