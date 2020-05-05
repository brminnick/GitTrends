namespace GitTrends
{
    public static class NotificationHubConstants
    {
#if !AppStore
        public const string Name = "GitTrends_Debug";
        public const string ListenConnectionString = "";
#else
        public const string Name = "GitTrends";
        public const string ListenConnectionString = "";
#endif
    }
}
