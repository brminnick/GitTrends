using System.Collections.Generic;
using Autofac;
using GitTrends.Shared;

namespace GitTrends
{
    public class ContainerService
    {
        public static IContainer Container { get; } = CreateContainer();

        static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            //Register Services
            builder.RegisterType<RepositoryDatabase>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubAuthenticationService>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubGraphQLApiService>().AsSelf().SingleInstance();
            builder.RegisterType<AzureFunctionsApiService>().AsSelf().SingleInstance();
            builder.RegisterType<SyncfusionService>().AsSelf().SingleInstance();
            builder.RegisterType<GitHubApiV3Service>().AsSelf().SingleInstance();

            //Register ViewModels
            builder.RegisterType<RepositoryViewModel>().AsSelf();
            builder.RegisterType<ProfileViewModel>().AsSelf();
            builder.RegisterType<TrendsViewModel>().AsSelf();

            //Register Pages
            builder.RegisterType<RepositoryPage>().AsSelf();
            builder.RegisterType<ProfilePage>().AsSelf();
            builder.RegisterType<TrendsPage>().AsSelf().WithParameter(new TypedParameter(typeof(Repository), "repository"));

            return builder.Build();
        }
    }
}
