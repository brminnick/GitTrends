using System.Net;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using GitHubApiStatus;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Plugin.StoreReview;
using Plugin.StoreReview.Abstractions;
using Polly;
using Refit;
using Sharpnado.MaterialFrame;
using Shiny;
using Syncfusion.Maui.Core.Hosting;

namespace GitTrends;

public static partial class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSharpnadoMaterialFrame(true)
			.UseShiny()
			.UseMauiCommunityToolkit()
			.UseMauiCommunityToolkitMarkup()
			.ConfigureSyncfusionCore()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddNotifications();

#if AppStore
#error: No Shiny Jobs defined
		builder.Services.AddJob(typeof(BackgroundFetchService));
#endif

		RegisterCrossPlatformAPIs(builder.Services);
		RegisterDatabases(builder.Services);
		RegisterPagesAndViewModels(builder.Services);
		RegisterHttpClientServices(builder.Services);
		RegisterServices(builder.Services);

		return builder.Build();
	}

	static void RegisterDatabases(in IServiceCollection services)
	{
		services.AddSingleton<RepositoryDatabase>();
		services.AddSingleton<ReferringSitesDatabase>();
	}

	static void RegisterCrossPlatformAPIs(in IServiceCollection services)
	{
		services.AddSingleton<IAppInfo>(AppInfo.Current);
		services.AddSingleton<IBrowser>(Browser.Default);
		services.AddSingleton<IDeviceInfo>(DeviceInfo.Current);
		services.AddSingleton<IEmail>(Email.Default);
		services.AddSingleton<IFileSystem>(FileSystem.Current);
		services.AddSingleton<ILauncher>(Launcher.Default);
		services.AddSingleton<IPreferences>(Preferences.Default);
		services.AddSingleton<ISecureStorage>(SecureStorage.Default);
		services.AddSingleton<IVersionTracking>(VersionTracking.Default);
	}

	static void RegisterServices(in IServiceCollection services)
	{
		services.AddSingleton<App>();
		services.AddSingleton<IAnalyticsService, AnalyticsService>();
		services.AddSingleton<AnalyticsInitializationService>();
		services.AddSingleton<AppInitializationService>();
		services.AddSingleton<AppStoreConstants>();
		services.AddSingleton<AzureFunctionsApiService>();
		services.AddSingleton<BackgroundFetchService>();
		services.AddSingleton<DeepLinkingService>();
		services.AddSingleton<FavIconService>();
		services.AddSingleton<FirstRunService>();
		services.AddSingleton<GitHubApiStatusService>();
		services.AddSingleton<GitHubApiRepositoriesService>();
		services.AddSingleton<GitHubApiV3Service>();
		services.AddSingleton<GitHubAuthenticationService>();
		services.AddSingleton<GitHubUserService>();
		services.AddSingleton<GitHubGraphQLApiService>();
		services.AddSingleton<GitTrendsStatisticsService>();
		services.AddSingleton<LanguageService>();
		services.AddSingleton<LibrariesService>();
		services.AddSingleton<MediaElementService>();
		services.AddSingleton<NotificationService>();
		services.AddSingleton<ReferringSitesDatabase>();
		services.AddSingleton<RepositoryDatabase>();
		services.AddSingleton<ReviewService>();
		services.AddSingleton<MobileSortingService>();
		services.AddSingleton<SyncfusionService>();
		services.AddSingleton<ThemeService>();
		services.AddSingleton<TrendsChartSettingsService>();
		services.AddSingleton<IStoreReview>(CrossStoreReview.Current);
#if !AppStore
		// #ToDo Register UI Test Service
#endif
	}

	static void RegisterPagesAndViewModels(in IServiceCollection services)
	{
		services.AddSingleton<App>();
		
		services.AddTransientWithShellRoute<AboutPage, AboutViewModel>();
		services.AddTransient<ChartOnboardingPage>();
		services.AddTransient<ConnectToGitHubOnboardingPage>();
		services.AddTransient<GitTrendsOnboardingPage>();
		services.AddTransient<NotificationsOnboardingPage>();
		services.AddTransient<OnboardingCarouselPage>();
		services.AddTransient<OnboardingViewModel>();
		services.AddTransientWithShellRoute<ReferringSitesPage, ReferringSitesViewModel>();
		services.AddTransientWithShellRoute<RepositoryPage, RepositoryViewModel>();
		services.AddTransientWithShellRoute<SettingsPage, SettingsViewModel>();
		services.AddTransient<SplashScreenPage>();
		services.AddTransient<StarsTrendsPage>();
		services.AddTransient<TrendsCarouselPage>();
		services.AddTransient<ViewsClonesTrendsPage>();
		services.AddTransient<TrendsViewModel>();
		services.AddTransient<WelcomePage>();
	}

	static void RegisterHttpClientServices(in IServiceCollection services)
	{
		services.AddRefitClient<IGitHubApiV3>()
							.ConfigureHttpClient(static client => client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl))
							.AddStandardResilienceHandler(static options => options.Retry = new MobileHttpRetryStrategyOptions());

		services.AddRefitClient<IGitHubGraphQLApi>()
							.ConfigureHttpClient(static client => client.BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi))
							.AddStandardResilienceHandler(static options => options.Retry = new MobileHttpRetryStrategyOptions());

		services.AddRefitClient<IAzureFunctionsApi>()
							.ConfigureHttpClient(static client => client.BaseAddress = new Uri(AzureConstants.AzureFunctionsApiUrl))
							.AddStandardResilienceHandler(static options => options.Retry = new MobileHttpRetryStrategyOptions());

		services.AddHttpClient<FavIconService>();
	}

	static IServiceCollection AddTransientWithShellRoute<TPage, TViewModel>(this IServiceCollection services) where TPage : BaseContentPage<TViewModel>
																												where TViewModel : BaseViewModel
	{
		return services.AddTransientWithShellRoute<TPage, TViewModel>(AppShell.GetPageRoute<TViewModel>());
	}

	sealed class MobileHttpRetryStrategyOptions : HttpRetryStrategyOptions
	{
		public MobileHttpRetryStrategyOptions()
		{
			BackoffType = DelayBackoffType.Exponential;
			MaxRetryAttempts = 3;
			UseJitter = true;
			Delay = TimeSpan.FromSeconds(2);
		}
	}
}