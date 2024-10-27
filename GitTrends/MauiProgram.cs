using System.Diagnostics;
using System.Net;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.ApplicationModel;
using CommunityToolkit.Maui.Markup;
using GitHubApiStatus;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Plugin.StoreReview;
using Plugin.StoreReview.Abstractions;
using Polly;
using Refit;
using Sentry.Maui;
using Sharpnado.MaterialFrame;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Core.Hosting;

#if ANDROID || IOS || MACCATALYST
using Shiny;
#endif

namespace GitTrends;

public static partial class MauiProgram
{
	public static MauiApp CreateMauiApp(IAppInfo appInfo)
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
			.UseSharpnadoMaterialFrame(true)
#if ANDROID || IOS || MACCATALYST
			.UseShiny()
#endif
			.UseMauiCommunityToolkit()
			.UseMauiCommunityToolkitMarkup()
			.UseMauiCommunityToolkitMediaElement()
			.UseSentry(options => ConfigureSentryOptions(options, appInfo))
			.ConfigureSyncfusionCore()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("FontAwesome.ttf", FontFamilyConstants.FontAwesome);
				fonts.AddFont("FontAwesomeSolid.ttf", FontFamilyConstants.FontAwesomeSolid);
				fonts.AddFont("FontAwesomeBrands.ttf", FontFamilyConstants.FontAwesomeBrands);
				fonts.AddFont("Roboto-Bold.ttf", FontFamilyConstants.RobotoBold);
				fonts.AddFont("Roboto-Medium.ttf", FontFamilyConstants.RobotoMedium);
				fonts.AddFont("Roboto-Regular.ttf", FontFamilyConstants.RobotoRegular);
			})
			.ConfigureMauiHandlers(handlers =>
			{
#if IOS || MACCATALYST
				handlers.AddHandler<Shell, ShellWithLargeTitlesHandler>();
#endif
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddLogging();

#if ANDROID || IOS || MACCATALYST
		builder.Services.AddNotifications();
		builder.Services.AddJob(typeof(CleanDatabaseJob));
		builder.Services.AddJob(typeof(NotifyTrendingRepositoriesJob));
		CustomizeHandlers();
#endif

		RegisterCrossPlatformAPIs(builder.Services);
		RegisterDatabases(builder.Services);
		RegisterPagesAndViewModels(builder.Services);
		RegisterHttpClientServices(builder.Services);
		RegisterServices(builder.Services);
		RegisterBackgroundJobs(builder.Services);

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
		services.AddSingleton<IBadge>(Badge.Default);
		services.AddSingleton<IBrowser>(Browser.Default);
		services.AddSingleton<IDeviceDisplay>(DeviceDisplay.Current);
		services.AddSingleton<IDeviceInfo>(DeviceInfo.Current);
		services.AddSingleton<IEmail>(Email.Default);
		services.AddSingleton<IFileSystem>(FileSystem.Current);
		services.AddSingleton<ILauncher>(Launcher.Default);
		services.AddSingleton<IPreferences>(Preferences.Default);
		services.AddSingleton<ISecureStorage>(SecureStorage.Default);
		services.AddSingleton<IStoreReview>(CrossStoreReview.Current);
		services.AddSingleton<IVersionTracking>(VersionTracking.Default);
	}

	static void RegisterServices(in IServiceCollection services)
	{
		services.AddSingleton<App>();
		services.AddSingleton<IAnalyticsService, AnalyticsService>();
		services.AddSingleton<AppInitializationService>();
		services.AddSingleton<AppStoreConstants>();
		services.AddSingleton<AzureFunctionsApiService>();
		services.AddSingleton<BackgroundFetchService>();
		services.AddSingleton<DeepLinkingService>();
		services.AddSingleton<FavIconService>();
		services.AddSingleton<FirstRunService>();
		services.AddSingleton<IGitHubApiStatusService>(_ => new GitHubApiStatusService());
		services.AddSingleton<INotificationPermissionStatus, NotificationPermissionService>();
		services.AddSingleton<GitHubApiRepositoriesService>();
		services.AddSingleton<GitHubApiV3Service>();
		services.AddSingleton<GitHubAuthenticationService>();
		services.AddSingleton<GitHubUserService>();
		services.AddSingleton<GitHubGraphQLApiService>();
		services.AddSingleton<GitTrendsStatisticsService>();
		services.AddSingleton<LanguageService>();
		services.AddSingleton<LibrariesService>();
		services.AddSingleton<NotificationService>();
		services.AddSingleton<ReferringSitesDatabase>();
		services.AddSingleton<RepositoryDatabase>();
		services.AddSingleton<ReviewService>();
		services.AddSingleton<MobileSortingService>();
		services.AddSingleton<SyncfusionService>();
		services.AddSingleton<ThemeService>();
		services.AddSingleton<TrendsChartSettingsService>();
	}

	static void RegisterPagesAndViewModels(in IServiceCollection services)
	{
		// App + AppShell
		services.AddSingleton<App>();
		services.AddSingleton<AppShell>();

		// Pages
		services.AddTransientWithShellRoute<AboutPage, AboutViewModel>();
		services.AddTransientWithShellRoute<OnboardingPage, OnboardingViewModel>();
		services.AddTransientWithShellRoute<ReferringSitesPage, ReferringSitesViewModel>();
		services.AddTransientWithShellRoute<RepositoryPage, RepositoryViewModel>();
		services.AddTransientWithShellRoute<SettingsPage, SettingsViewModel>();
		services.AddTransientWithShellRoute<TrendsPage, TrendsViewModel>();
		services.AddTransientWithShellRoute<WelcomePage, WelcomeViewModel>();
		services.AddTransient<SplashScreenPage>();

		// Views
		services.AddTransient<StarsTrendsView>();
		services.AddTransient<ViewsClonesTrendsView>();
		services.AddTransient<ChartOnboardingView>();
		services.AddTransient<ConnectToGitHubOnboardingView>();
		services.AddTransient<GitTrendsOnboardingView>();
		services.AddTransient<NotificationsOnboardingView>();
	}

	static void RegisterHttpClientServices(in IServiceCollection services)
	{
		services.AddRefitClient<IGitHubApiV3>()
			.ConfigureHttpClient(static client => client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl))
			.ConfigurePrimaryHttpMessageHandler(static () => new HttpClientHandler
			{
				AutomaticDecompression = GetDecompressionMethods()
			})
			.AddStandardResilienceHandler(static options => options.Retry = new MobileHttpRetryStrategyOptions());

		services.AddRefitClient<IGitHubGraphQLApi>()
			.ConfigureHttpClient(static client => client.BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi))
			.ConfigurePrimaryHttpMessageHandler(static () => new HttpClientHandler
			{
				AutomaticDecompression = GetDecompressionMethods()
			})
			.AddStandardResilienceHandler(static options => options.Retry = new MobileHttpRetryStrategyOptions());

		services.AddRefitClient<IAzureFunctionsApi>()
			.ConfigureHttpClient(static client => client.BaseAddress = new Uri(AzureConstants.AzureFunctionsApiUrl))
			.ConfigurePrimaryHttpMessageHandler(static () => new HttpClientHandler
			{
				AutomaticDecompression = GetDecompressionMethods()
			})
			.AddStandardResilienceHandler(static options => options.Retry = new MobileHttpRetryStrategyOptions());

		services.AddHttpClient<FavIconService>()
			.ConfigurePrimaryHttpMessageHandler(static () => new HttpClientHandler
			{
				AutomaticDecompression = GetDecompressionMethods()
			});

		static DecompressionMethods GetDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli;
	}

	static void RegisterBackgroundJobs(in IServiceCollection services)
	{
		services.AddTransient<CleanDatabaseJob>();
		services.AddTransient<RetryRepositoryStarsJob>();
		services.AddTransient<RetryGetReferringSitesJob>();
		services.AddTransient<NotifyTrendingRepositoriesJob>();
		services.AddTransient<RetryOrganizationsRepositoriesJob>();
		services.AddTransient<RetryRepositoriesViewsClonesStarsJob>();
	}

	static IServiceCollection AddTransientWithShellRoute<TPage, TViewModel>(this IServiceCollection services) where TPage : BaseContentPage
		where TViewModel : BaseViewModel
	{
		return services.AddTransientWithShellRoute<TPage, TViewModel>(AppShell.GetPageRoute<TPage>());
	}


	static void ConfigureSentryOptions(in SentryMauiOptions options, in IAppInfo appInfo)
	{
		options.TracesSampleRate = 1.0;
		options.IncludeTextInBreadcrumbs = true;
		options.IncludeTitleInBreadcrumbs = true;
		options.IncludeBackgroundingStateInBreadcrumbs = true;
		options.StackTraceMode = StackTraceMode.Enhanced;
		options.IsGlobalModeEnabled = true;
		options.Release = appInfo.VersionString;
		options.Distribution = appInfo.BuildString;

		options.TracesSampleRate = 1.0;
		options.ProfilesSampleRate = 1.0;

		options.ExperimentalMetrics = new ExperimentalMetricsOptions
		{
			EnableCodeLocations = true
		};

		ConfigureDebugSentryOptions(options);
		ConfigureReleaseSentryOptions(options);
		ConfigureAppStoreSentryOptions(options);

		[Conditional("DEBUG")]
		static void ConfigureDebugSentryOptions(SentryMauiOptions options)
		{
			options.Dsn = "https://4e21564ab4374deb8b95da8a25dc2cca@o166840.ingest.us.sentry.io/6568237";
			options.Debug = true;
			options.Environment = "DEBUG";
			options.DiagnosticLevel = SentryLevel.Debug;
		}

		[Conditional("RELEASE")]
		static void ConfigureReleaseSentryOptions(SentryMauiOptions options)
		{
			options.Dsn = "https://4e21564ab4374deb8b95da8a25dc2cca@o166840.ingest.us.sentry.io/6568237";
			options.Environment = "RELEASE";
		}

		[Conditional("AppStore")]
		static void ConfigureAppStoreSentryOptions(SentryMauiOptions options)
		{
			options.Dsn = "https://8832afb11833071cabf263932c80a10d@o166840.ingest.us.sentry.io/4507805605363712";
			options.Environment = "AppStore";
		}
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