namespace GitTrends.Shared
{
    public record NotificationHubInformation(string Name, string ConnectionString, string Name_Debug, string ConnectionString_Debug)
    {
        public static NotificationHubInformation Empty { get; } = new NotificationHubInformation(string.Empty, string.Empty, string.Empty, string.Empty);
    }

    public static class NotificationHubInformationExtensions
    {
        public static bool IsEmpty(this NotificationHubInformation notificationHubDTO) => notificationHubDTO.Name == string.Empty
                                                                                            && notificationHubDTO.ConnectionString == string.Empty
                                                                                            && notificationHubDTO.Name_Debug == string.Empty
                                                                                            && notificationHubDTO.ConnectionString_Debug == string.Empty;
    }
}
