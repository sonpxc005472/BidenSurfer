using System;

namespace BidenSurfer.Infras
{
    public enum BotStatusEnums
    {
        Pending = 0,
        Active = 1,
        Archive = 2
    }

    public enum UserRoleEnums
    {
        Admin = 1,
        Trader = 2
    }

    public enum UserStatusEnums
    {
        Pending = 0,
        Active = 1,
        Deleted = 2
    }

    public enum OrderStatusEnums
    {
        Uncompleted = 0,
        Completed = 1,
        Partial = 2,
        Deleted = 3
    }

    public enum OrderTypeEnums
    {
        Spot = 0,
        Margin = 1,
        Perpetual = 2
    }

    public static class EnumHelpers
    {
        public static string GetUserRoleName(this int role)
        {
            return Enum.GetName(typeof(UserRoleEnums), role) ?? string.Empty;
        }
    }
}
