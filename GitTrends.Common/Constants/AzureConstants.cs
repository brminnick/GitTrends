namespace  GitTrends.Common;

public static class AzureConstants
{
#if AppStore
#error Missing API Keys
#endif
	public const string GetTestTokenApiKey = "";
	public const string GetAppCenterApiKeysKey = "";
	public const string GetSyncFusionInformationApiKey = "";
	public const string GetNotificationHubInformationApiKey = "";
	public const string AzureFunctionsApiUrl = "https://gittrends-functions.azurewebsites.net/api";
}