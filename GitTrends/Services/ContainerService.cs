using System.Collections.Generic;
using Autofac;

namespace GitTrends
{
    public class ContainerService
    {
        public static IContainer Container { get; } = CreateContainer();

        static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            //Register Services
            builder.RegisterType<RepositoryDatabase>().AsSelf();
            builder.RegisterType<GitHubAuthenticationService>().AsSelf();
            builder.RegisterType<GitHubGraphQLApiService>().AsSelf();
            builder.RegisterType<AzureFunctionsApiService>().AsSelf();
            builder.RegisterType<SyncfusionService>().AsSelf();

            //Register ViewModels
            builder.RegisterType<RepositoryViewModel>().AsSelf();
            builder.RegisterType<ProfileViewModel>().AsSelf();
            builder.RegisterType<TrendsViewModel>().AsSelf();

            //Register Pages
            builder.RegisterType<RepositoryPage>().AsSelf();
            builder.RegisterType<ProfilePage>().AsSelf();
            builder.RegisterType<TrendsPage>().AsSelf().WithParameters(new List<TypedParameter>
            {
                new TypedParameter(typeof(string), "owner"),
                new TypedParameter(typeof(string), "repository"),
            });

            return builder.Build();
        }
    }
}
