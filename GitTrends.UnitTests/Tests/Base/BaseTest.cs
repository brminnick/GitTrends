using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shiny.Notifications;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends.UnitTests
{
    abstract class BaseTest
    {
        protected const string AuthenticatedGitHubUserLogin = "brminnick";
        protected const string AuthenticatedGitHubUserName = "Brandon Minnick";
        protected const string AuthenticatedGitHubUserAvatarUrl = "https://avatars0.githubusercontent.com/u/13558917?u=f1392f8aefe2d52a87c4d371981cb7153199fa27&v=4";
        protected const string ValidGitHubRepo = "GitTrends";

        [SetUp]
        public virtual async Task Setup()
        {
            Device.Info = new MockDeviceInfo();
            Device.PlatformServices = new MockPlatformServices();

            var preferences  = ServiceCollection.ServiceProvider.GetService<IPreferences>();
            preferences.Clear();

            var secureStorage = ServiceCollection.ServiceProvider.GetService<ISecureStorage>();
            secureStorage.RemoveAll();

            var referringSitesDatabase = ServiceCollection.ServiceProvider.GetService<ReferringSitesDatabase>();
            await referringSitesDatabase.DeleteAllData().ConfigureAwait(false);

            var repositoryDatabase = ServiceCollection.ServiceProvider.GetService<RepositoryDatabase>();
            await repositoryDatabase.DeleteAllData().ConfigureAwait(false);

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetService<GitHubAuthenticationService>();
            await gitHubAuthenticationService.LogOut().ConfigureAwait(false);

            var notificationService = ServiceCollection.ServiceProvider.GetService<NotificationService>();
            await notificationService.SetAppBadgeCount(0).ConfigureAwait(false);
            notificationService.UnRegister();

            var mockNotificationService = (MockNotificationService)ServiceCollection.ServiceProvider.GetService<INotificationService>();
            mockNotificationService.Reset();
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
