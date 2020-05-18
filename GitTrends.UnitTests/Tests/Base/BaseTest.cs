using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Xamarin.Forms;

namespace GitTrends.UnitTests
{
    abstract class BaseTest
    {
        [SetUp]
        public virtual async Task Setup()
        {
            Device.Info = new MockDeviceInfo();
            Device.PlatformServices = new MockPlatformServices();

            var referringSitesDatabase = ContainerService.Container.GetService<ReferringSitesDatabase>();
            await referringSitesDatabase.DeleteAllData().ConfigureAwait(false);

            var repositoryDatabase = ContainerService.Container.GetService<RepositoryDatabase>();
            await repositoryDatabase.DeleteAllData().ConfigureAwait(false);
        }

        protected static async Task AuthenticateUser(GitHubUserService gitHubUserService, GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            var uiTestToken = await Mobile.Shared.AzureFunctionsApiService.GetUITestToken().ConfigureAwait(false);
            await gitHubUserService.SaveGitHubToken(uiTestToken).ConfigureAwait(false);

            var (login, name, avatarUri) = await gitHubGraphQLApiService.GetCurrentUserInfo(CancellationToken.None).ConfigureAwait(false);

            gitHubUserService.Alias = login;
            gitHubUserService.Name = name;
            gitHubUserService.AvatarUrl = avatarUri.ToString();
        }
    }
}
