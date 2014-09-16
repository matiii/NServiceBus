namespace NServiceBus.Timeout
{
    class TimeoutManagerHeaders
    {
        public const string Expire = "NServiceBus.Timeout.Expire";
        public const string RouteExpiredTimeoutTo = "NServiceBus.Timeout.RouteExpiredTimeoutTo";
        public const string ClearTimeouts = "NServiceBus.ClearTimeouts";
    }
}