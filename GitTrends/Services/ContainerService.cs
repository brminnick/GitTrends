using System;
using Autofac;
using GitTrends.Shared;

namespace GitTrends
{
    public static class ContainerService
    {
        readonly static Lazy<IContainer> _containerHolder = new Lazy<IContainer>(CreateContainer);

        public static IContainer Container => _containerHolder.Value;

        static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            //Register Services
            builder.RegisterType<AnalyticsService>().AsSelf().SingleInstance();
            builder.RegisterType<AzureFunctionsApiService>().AsSelf().SingleInstance();
            builder.RegisterType<DeepLinkingService>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubApiV3Service>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubAuthenticationService>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubGraphQLApiService>().AsSelf().SingleInstance();
            builder.RegisterType<RepositoryDatabase>().AsSelf().SingleInstance();
            builder.RegisterType<ReviewService>().AsSelf().SingleInstance();
            builder.RegisterType<SyncFusionService>().AsSelf().SingleInstance();
            builder.RegisterType<TrendsChartSettingsService>().AsSelf().SingleInstance();
            builder.RegisterType<SortingService>().AsSelf().SingleInstance();
#if !AppStore
            builder.RegisterType<UITestBackdoorService>().AsSelf().SingleInstance();
#endif

            //Register ViewModels
            builder.RegisterType<ReferringSitesViewModel>().AsSelf();
            builder.RegisterType<RepositoryViewModel>().AsSelf();
            builder.RegisterType<SettingsViewModel>().AsSelf();
            builder.RegisterType<SplashScreenViewModel>().AsSelf();
            builder.RegisterType<TrendsViewModel>().AsSelf();

            //Register Pages
            builder.RegisterType<ReferringSitesPage>().AsSelf().WithParameter(new TypedParameter(typeof(Repository), nameof(Repository).ToLower()));
            builder.RegisterType<RepositoryPage>().AsSelf();
            builder.RegisterType<SettingsPage>().AsSelf();
            builder.RegisterType<SplashScreenPage>().AsSelf();
            builder.RegisterType<TrendsPage>().AsSelf().WithParameter(new TypedParameter(typeof(Repository), nameof(Repository).ToLower()));

            return builder.Build();
        }
    }
}
