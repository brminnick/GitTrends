namespace GitTrends.Shared
{
    class NotificationHubDTO
    {
        public NotificationHubDTO(string name, string connectionString, string name_debug, string connectionString_debug) =>
            (Name, ConnectionString, Name_Debug, ConnectionString_Debug) = (name, connectionString, name_debug, connectionString_debug);

        public string Name { get; }
        public string ConnectionString { get; }

        public string Name_Debug { get; }
        public string ConnectionString_Debug { get; }
    }
}
