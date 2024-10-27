using System.Net;
using System.Net.Http.Headers;
using CommunityToolkit.Maui.ApplicationModel;
using GitHubApiStatus;
using GitHubApiStatus.Extensions;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using Plugin.StoreReview.Abstractions;
using Shiny.Jobs;
using Shiny.Notifications;

namespace GitTrends.UnitTests;

static class ServiceCollection
{
	static IServiceProvider? _serviceProviderHolder;

	public static IServiceProvider ServiceProvider => _serviceProviderHolder ?? throw new InvalidOperationException("Must call Initialize first");

	public static void Initialize(IAzureFunctionsApi azureFunctionsApi, IGitHubApiV3 gitHubApiV3, IGitHubGraphQLApi gitHubGraphQLApi) =>
		_serviceProviderHolder = CreateContainer(azureFunctionsApi, gitHubApiV3, gitHubGraphQLApi);

	static ServiceProvider CreateContainer(IAzureFunctionsApi azureFunctionsApi, IGitHubApiV3 gitHubApiV3, IGitHubGraphQLApi gitHubGraphQLApi)
	{
		var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

		// GitTrends Refit Services
		services.AddHttpClient();
		services.AddSingleton(gitHubApiV3);
		services.AddSingleton(gitHubGraphQLApi);
		services.AddSingleton(azureFunctionsApi);


		// GitTrends Services
		services.AddSingleton<AppInitializationService>();
		services.AddSingleton<AzureFunctionsApiService>();
		services.AddSingleton<DeepLinkingService>();
		services.AddSingleton<BackgroundFetchService, ExtendedBackgroundFetchService>();
		services.AddSingleton<NotificationService, ExtendedNotificationService>();
		services.AddSingleton<FavIconService>();
		services.AddSingleton<FirstRunService>();
		services.AddSingleton<GitHubApiV3Service>();
		services.AddSingleton<GitHubApiRepositoriesService>();
		services.AddSingleton<GitHubAuthenticationService>();
		services.AddSingleton<GitHubGraphQLApiService>();
		services.AddSingleton<GitHubUserService>();
		services.AddSingleton<GitTrendsStatisticsService>();
		services.AddSingleton<IGitHubApiStatusService>(_ => new GitHubApiStatusService());
		services.AddSingleton<LanguageService>();
		services.AddSingleton<LibrariesService>();
		services.AddSingleton<ReferringSitesDatabase>();
		services.AddSingleton<RepositoryDatabase>();
		services.AddSingleton<ReviewService>();
		services.AddSingleton<MobileSortingService>();
		services.AddSingleton<SyncfusionService>();
		services.AddSingleton<ThemeService>();
		services.AddSingleton<TrendsChartSettingsService>();

		// GitTrends ViewModels
		services.AddTransient<AboutViewModel>();
		services.AddTransient<OnboardingViewModel>();
		services.AddTransient<ReferringSitesViewModel, ExtendedReferringSitesViewModel>();
		services.AddTransient<RepositoryViewModel>();
		services.AddTransient<SettingsViewModel>();
		services.AddTransient<TrendsViewModel>();
		services.AddTransient<WelcomeViewModel>();

		// Background Jobs
		services.AddTransient<CleanDatabaseJob>();
		services.AddTransient<RetryRepositoryStarsJob>();
		services.AddTransient<RetryGetReferringSitesJob>();
		services.AddTransient<NotifyTrendingRepositoriesJob>();
		services.AddTransient<RetryOrganizationsRepositoriesJob>();
		services.AddTransient<RetryRepositoriesViewsClonesStarsJob>();

		// Mocks
		services.AddSingleton<IAnalyticsService, MockAnalyticsService>();
		services.AddSingleton<IAppInfo, MockAppInfo>();
		services.AddSingleton<IBadge, MockBadge>();
		services.AddSingleton<IBrowser, MockBrowser>();
		services.AddSingleton<IDeviceInfo, MockDeviceInfo>();
		services.AddSingleton<IDispatcher, MockDispatcher>();
		services.AddSingleton<IFileSystem, MockFileSystem>();
		services.AddSingleton<IEmail, MockEmail>();
		services.AddSingleton<ILauncher, MockLauncher>();
		services.AddSingleton<IJobManager, MockJobManager>();
		services.AddSingleton<INotificationManager, MockNotificationManager>();
		services.AddSingleton<INotificationPermissionStatus, MockNotificationPermissionService>();
		services.AddSingleton<ISecureStorage, MockSecureStorage>();
		services.AddSingleton<IStoreReview, MockStoreReview>();
		services.AddSingleton<IPreferences, MockPreferences>();
		services.AddSingleton<IVersionTracking, MockVersionTracking>();

		return services.BuildServiceProvider();
	}
}