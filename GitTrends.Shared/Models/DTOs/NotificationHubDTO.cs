namespace GitTrends.Shared
{
    public class NotificationHubInformation
    {
        public NotificationHubInformation(string name, string connectionString, string name_debug, string connectionString_debug) =>
            (Name, ConnectionString, Name_Debug, ConnectionString_Debug) = (name, connectionString, name_debug, connectionString_debug);

        public static NotificationHubInformation Empty { get; } = new NotificationHubInformation(string.Empty, string.Empty, string.Empty, string.Empty);

        public string Name { get; }
        public string ConnectionString { get; }

        public string Name_Debug { get; }
        public string ConnectionString_Debug { get; }
    }

    public static class NotificationHubInformationExtensions
    {
        public static bool IsEmpty(this NotificationHubInformation notificationHubDTO)
        {
            return notificationHubDTO.Name == string.Empty
                    && notificationHubDTO.ConnectionString == string.Empty
                    && notificationHubDTO.Name_Debug == string.Empty
                    && notificationHubDTO.ConnectionString_Debug == string.Empty;
        }
    }
}
