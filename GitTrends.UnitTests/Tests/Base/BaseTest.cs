using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends.UnitTests
{
    abstract class BaseTest
    {
        protected const string AuthenticatedGitHubUserAvatarUrl = "https://avatars0.githubusercontent.com/u/13558917?u=f1392f8aefe2d52a87c4d371981cb7153199fa27&v=4";
        protected const string GitTrendsRepoName = "GitTrends";
        protected const string GitTrendsRepoOwner = "brminnick";

        [TearDown]
        public virtual Task TearDown() => Task.CompletedTask;

        [SetUp]
        public virtual async Task Setup()
        {
            CultureInfo.DefaultThreadCurrentCulture = null;
            CultureInfo.DefaultThreadCurrentUICulture = null;

            Device.Info = new MockDeviceInfo();
            Device.PlatformServices = new MockPlatformServices();

            var preferences = ServiceCollection.ServiceProvider.GetRequiredService<IPreferences>();
            preferences.Clear();

            var secureStorage = ServiceCollection.ServiceProvider.GetRequiredService<ISecureStorage>();
            secureStorage.RemoveAll();

            var referringSitesDatabase = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesDatabase>();
            await referringSitesDatabase.DeleteAllData().ConfigureAwait(false);

            var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
            await repositoryDatabase.DeleteAllData().ConfigureAwait(false);

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
            await gitHubAuthenticationService.LogOut().ConfigureAwait(false);

            var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();
            await notificationService.SetAppBadgeCount(0).ConfigureAwait(false);
            notificationService.UnRegister();

            var mockNotificationService = (MockDeviceNotificationsService)ServiceCollection.ServiceProvider.GetRequiredService<IDeviceNotificationsService>();
            mockNotificationService.Reset();
        }

        protected static async Task AuthenticateUser(GitHubUserService gitHubUserService, GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            var token = await Mobile.Common.AzureFunctionsApiService.GetTestToken().ConfigureAwait(false);
            await gitHubUserService.SaveGitHubToken(token).ConfigureAwait(false);

            var (login, name, avatarUri) = await gitHubGraphQLApiService.GetCurrentUserInfo(CancellationToken.None).ConfigureAwait(false);

            gitHubUserService.Alias = login;
            gitHubUserService.Name = name;
            gitHubUserService.AvatarUrl = avatarUri.ToString();
        }

        protected static Repository CreateRepository(bool createViewsAndClones = true)
        {
            const string gitTrendsAvatarUrl = "https://avatars3.githubusercontent.com/u/61480020?s=400&u=b1a900b5fa1ede22af9d2d9bfd6c49a072e659ba&v=4";
            var downloadedAt = DateTimeOffset.UtcNow;

            var dailyViewsList = new List<DailyViewsModel>();
            var dailyClonesList = new List<DailyClonesModel>();

            for (int i = 0; i < 14 && createViewsAndClones; i++)
            {
                var count = DemoDataConstants.GetRandomNumber();
                var uniqeCount = count / 2; //Ensures uniqueCount is always less than count

                dailyViewsList.Add(new DailyViewsModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqeCount));
                dailyClonesList.Add(new DailyClonesModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqeCount));
            }

            return new Repository($"Repository " + DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomNumber(),
                                                        new RepositoryOwner(DemoUserConstants.Alias, gitTrendsAvatarUrl),
                                                        new IssuesConnection(DemoDataConstants.GetRandomNumber(), Enumerable.Empty<Issue>()),
                                                        gitTrendsAvatarUrl, new StarGazers(DemoDataConstants.GetRandomNumber()), false, downloadedAt, false, dailyViewsList, dailyClonesList);
        }
    }
}
