using System.Net;
using System.Net.Http.Headers;
using GitHubApiStatus;
using GitHubApiStatus.Extensions;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Plugin.StoreReview.Abstractions;
using Shiny.Jobs;
using Shiny.Notifications;

namespace GitTrends.UnitTests;

static class ServiceCollection
{
	static IServiceProvider? _serviceProviderHolder;

	public static IServiceProvider ServiceProvider => _serviceProviderHolder ?? throw new InvalidOperationException("Must call Initialize first");

	public static void Initialize(IAzureFunctionsApi azureFunctionsApi, IGitHubApiV3 gitHubApiV3, IGitHubGraphQLApi gitHubGraphQLApi, GitHubToken token) =>
		_serviceProviderHolder = CreateContainer(azureFunctionsApi, gitHubApiV3, gitHubGraphQLApi, token);
	
	public static DecompressionMethods GetDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli;

	static ServiceProvider CreateContainer(IAzureFunctionsApi azureFunctionsApi, IGitHubApiV3 gitHubApiV3, IGitHubGraphQLApi gitHubGraphQLApi, GitHubToken token)
	{
		var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

		//GitTrends Refit Services
		services.AddSingleton(gitHubApiV3);
		services.AddSingleton(gitHubGraphQLApi);
		services.AddSingleton(azureFunctionsApi);
		
		services.AddGitHubApiStatusService(new AuthenticationHeaderValue(token.TokenType, token.AccessToken), new ProductHeaderValue(nameof(GitTrends)))
			.ConfigurePrimaryHttpMessageHandler(static () => new HttpClientHandler
			{
				AutomaticDecompression = GetDecompressionMethods()
			});

		//GitTrends Services
		services.AddSingleton<AppInitializationService>();
		services.AddSingleton<AzureFunctionsApiService>();
		services.AddSingleton<BackgroundFetchService, ExtendedBackgroundFetchService>();
		services.AddSingleton<DeepLinkingService>();
		services.AddSingleton<NotificationService, ExtendedNotificationService>();
		services.AddSingleton<GitHubApiV3Service>();
		services.AddSingleton<GitHubApiRepositoriesService>();
		services.AddSingleton<GitHubAuthenticationService>();
		services.AddSingleton<GitHubGraphQLApiService>();
		services.AddSingleton<GitHubUserService>();
		services.AddSingleton<GitTrendsStatisticsService>();
		services.AddSingleton<FavIconService>();
		services.AddSingleton<FirstRunService>();
		services.AddSingleton<LanguageService>();
		services.AddSingleton<LibrariesService>();
		services.AddSingleton<MediaElementService>();
		services.AddSingleton<ReferringSitesDatabase>();
		services.AddSingleton<RepositoryDatabase>();
		services.AddSingleton<ReviewService>();
		services.AddSingleton<MobileSortingService>();
		services.AddSingleton<SyncfusionService>();
		services.AddSingleton<ThemeService>();
		services.AddSingleton<TrendsChartSettingsService>();

		//GitTrends ViewModels
		services.AddTransient<AboutViewModel>();
		services.AddTransient<OnboardingViewModel>();
		services.AddTransient<ReferringSitesViewModel>();
		services.AddTransient<RepositoryViewModel>();
		services.AddTransient<SettingsViewModel>();
		services.AddTransient<TrendsViewModel>();
		services.AddTransient<WelcomeViewModel>();

		//Mocks
		services.AddSingleton<IAnalyticsService, MockAnalyticsService>();
		services.AddSingleton<IAppInfo, MockAppInfo>();
		services.AddSingleton<IBrowser, MockBrowser>();
		services.AddSingleton<IDeviceInfo, MockDeviceInfo>();
		services.AddSingleton<IDispatcher, MockDispatcher>();
		services.AddSingleton<IDeviceNotificationsService, MockDeviceNotificationsService>();
		services.AddSingleton<IFileSystem, MockFileSystem>();
		services.AddSingleton<IEmail, MockEmail>();
		services.AddSingleton<ILauncher, MockLauncher>();
		services.AddSingleton<IJobManager, MockJobManager>();
		services.AddSingleton<INotificationManager, MockNotificationManager>();
		services.AddSingleton<ISecureStorage, MockSecureStorage>();
		services.AddSingleton<IStoreReview, MockStoreReview>();
		services.AddSingleton<IPreferences, MockPreferences>();
		services.AddSingleton<IVersionTracking, MockVersionTracking>();

		return services.BuildServiceProvider();
	}
}