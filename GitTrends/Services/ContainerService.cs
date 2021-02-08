using System;
using Autofac;
using GitHubApiStatus;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Shiny;
using Shiny.Notifications;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public static class ContainerService
    {
        readonly static Lazy<IContainer> _containerHolder = new(CreateContainer);

        public static IContainer Container => _containerHolder.Value;

        static IContainer CreateContainer()
        {
            Device.SetFlags(new[] { "Markup_Experimental", "IndicatorView_Experimental", "AppTheme_Experimental", "SwipeView_Experimental" });

            var builder = new ContainerBuilder();

            //Register Xamarin.Essentials
            builder.RegisterType<AppInfoImplementation>().As<IAppInfo>().SingleInstance();
            builder.RegisterType<BrowserImplementation>().As<IBrowser>().SingleInstance();
            builder.RegisterType<EmailImplementation>().As<IEmail>().SingleInstance();
            builder.RegisterType<FileSystemImplementation>().As<IFileSystem>().SingleInstance();
            builder.RegisterType<LauncherImplementation>().As<ILauncher>().SingleInstance();
            builder.RegisterType<MainThreadImplementation>().As<IMainThread>().SingleInstance();
            builder.RegisterType<PreferencesImplementation>().As<IPreferences>().SingleInstance();
            builder.RegisterType<SecureStorageImplementation>().As<ISecureStorage>().SingleInstance();
            builder.RegisterType<VersionTrackingImplementation>().As<IVersionTracking>().SingleInstance();

            //Register Services
            builder.RegisterType<AnalyticsService>().As<IAnalyticsService>().SingleInstance();
            builder.RegisterType<AzureFunctionsApiService>().AsSelf().SingleInstance();
            builder.RegisterType<BackgroundFetchService>().AsSelf().SingleInstance();
            builder.RegisterType<DeepLinkingService>().AsSelf().SingleInstance();
            builder.RegisterType<FavIconService>().AsSelf().SingleInstance();
            builder.RegisterType<FirstRunService>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubApiStatusService>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubApiRepositoriesService>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubApiV3Service>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubAuthenticationService>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubUserService>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubGraphQLApiService>().AsSelf().SingleInstance();
            builder.RegisterType<GitTrendsContributorsService>().AsSelf().SingleInstance();
            builder.RegisterType<ImageCachingService>().AsSelf().SingleInstance();
            builder.RegisterType<LanguageService>().AsSelf().SingleInstance();
            builder.RegisterType<MediaElementService>().AsSelf().SingleInstance();
            builder.RegisterType<NotificationService>().AsSelf().SingleInstance();
            builder.RegisterType<ReferringSitesDatabase>().AsSelf().SingleInstance();
            builder.RegisterType<RepositoryDatabase>().AsSelf().SingleInstance();
            builder.RegisterType<ReviewService>().AsSelf().SingleInstance();
            builder.RegisterType<MobileSortingService>().AsSelf().SingleInstance();
            builder.RegisterType<SyncfusionService>().AsSelf().SingleInstance();
            builder.RegisterType<ThemeService>().AsSelf().SingleInstance();
            builder.RegisterType<TrendsChartSettingsService>().AsSelf().SingleInstance();
            builder.RegisterInstance(ShinyHost.Resolve<INotificationManager>()).As<INotificationManager>().SingleInstance();
            builder.RegisterInstance(DependencyService.Resolve<IDeviceNotificationsService>()).As<IDeviceNotificationsService>().SingleInstance();
#if !AppStore
            builder.RegisterType<UITestsBackdoorService>().AsSelf().SingleInstance();
#endif

            //Register ViewModels
            builder.RegisterType<AboutViewModel>().AsSelf();
            builder.RegisterType<OnboardingViewModel>().AsSelf();
            builder.RegisterType<ReferringSitesViewModel>().AsSelf();
            builder.RegisterType<RepositoryViewModel>().AsSelf();
            builder.RegisterType<SettingsViewModel>().AsSelf();
            builder.RegisterType<SplashScreenViewModel>().AsSelf();
            builder.RegisterType<TrendsViewModel>().AsSelf();
            builder.RegisterType<WelcomeViewModel>().AsSelf();

            //Register Pages
            builder.RegisterType<AboutPage>().AsSelf();
            builder.RegisterType<ChartOnboardingPage>().AsSelf();
            builder.RegisterType<ConnectToGitHubOnboardingPage>().AsSelf();
            builder.RegisterType<GitTrendsOnboardingPage>().AsSelf();
            builder.RegisterType<NotificationsOnboardingPage>().AsSelf();
            builder.RegisterType<OnboardingCarouselPage>().AsSelf();
            builder.RegisterType<ReferringSitesPage>().AsSelf().WithParameter(new TypedParameter(typeof(Repository), nameof(Repository).ToLower()));
            builder.RegisterType<RepositoryPage>().AsSelf();
            builder.RegisterType<SettingsPage>().AsSelf();
            builder.RegisterType<SplashScreenPage>().AsSelf();
            builder.RegisterType<StarsTrendsPage>().AsSelf();
            builder.RegisterType<TrendsCarouselPage>().AsSelf().WithParameter(new TypedParameter(typeof(Repository), nameof(Repository).ToLower()));
            builder.RegisterType<ViewsClonesTrendsPage>().AsSelf();
            builder.RegisterType<WelcomePage>().AsSelf();

            //Register Refit Services
            IGitHubApiV3 gitHubV3ApiClient = RefitExtensions.For<IGitHubApiV3>(BaseApiService.CreateHttpClient(GitHubConstants.GitHubRestApiUrl));
            IGitHubGraphQLApi gitHubGraphQLApiClient = RefitExtensions.For<IGitHubGraphQLApi>(BaseApiService.CreateHttpClient(GitHubConstants.GitHubGraphQLApi));
            IAzureFunctionsApi azureFunctionsApiClient = RefitExtensions.For<IAzureFunctionsApi>(BaseApiService.CreateHttpClient(AzureConstants.AzureFunctionsApiUrl)); 

            builder.RegisterInstance(gitHubV3ApiClient).SingleInstance();
            builder.RegisterInstance(gitHubGraphQLApiClient).SingleInstance();
            builder.RegisterInstance(azureFunctionsApiClient).SingleInstance();

            return builder.Build();
        }
    }
}
