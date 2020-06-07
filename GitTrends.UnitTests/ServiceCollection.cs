using System;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using Shiny.Notifications;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    public class ServiceCollection
    {
        readonly static Lazy<IServiceProvider> _serviceProviderHolder = new Lazy<IServiceProvider>(CreateContainer);

        public static IServiceProvider ServiceProvider => _serviceProviderHolder.Value;

        static IServiceProvider CreateContainer()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            //GitTrends Services
            services.AddSingleton<AzureFunctionsApiService>();
            services.AddSingleton<BackgroundFetchService>();
            services.AddSingleton<DeepLinkingService>();
            services.AddSingleton<NotificationService, ExtendedNotificationService>();
            services.AddSingleton<GitHubApiV3Service>();
            services.AddSingleton<GitHubAuthenticationService>();
            services.AddSingleton<GitHubGraphQLApiService>();
            services.AddSingleton<GitHubUserService>();
            services.AddSingleton<FavIconService>();
            services.AddSingleton<FirstRunService>();
            services.AddSingleton<MediaElementService>();
            services.AddSingleton<ReferringSitesDatabase>();
            services.AddSingleton<RepositoryDatabase>();
            services.AddSingleton<ReviewService>();
            services.AddSingleton<SortingService>();
            services.AddSingleton<SyncfusionService>();
            services.AddSingleton<ThemeService>();
            services.AddSingleton<TrendsChartSettingsService>();

            //GitTrends ViewModels
            services.AddTransient<OnboardingViewModel>();
            services.AddTransient<ReferringSitesViewModel>();
            services.AddTransient<RepositoryViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SplashScreenViewModel>();
            services.AddTransient<TrendsViewModel>();
            services.AddTransient<WelcomeViewModel>();

            //Mocks
            services.AddSingleton<IAnalyticsService, MockAnalyticsService>();
            services.AddSingleton<IAppInfo, MockAppInfo>();
            services.AddSingleton<IBrowser, MockBrowser>();
            services.AddSingleton<IFileSystem, MockFileSystem>();
            services.AddSingleton<IEmail, MockEmail>();
            services.AddSingleton<ILauncher, MockLauncher>();
            services.AddSingleton<IMainThread, MockMainThread>();
            services.AddSingleton<INotificationService, MockNotificationService>();
            services.AddSingleton<INotificationManager, MockNotificationManager>();
            services.AddSingleton<ISecureStorage, MockSecureStorage>();
            services.AddSingleton<IPreferences, MockPreferences>();
            services.AddSingleton<IVersionTracking, MockVersionTracking>();

            return services.BuildServiceProvider();
        }
    }
}
