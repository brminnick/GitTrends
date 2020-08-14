using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class RepositoryViewModelTests : BaseTest
    {
        RepositoryViewModel? _repositoryViewModel;
        RepositoryViewModel RepositoryViewModel => _repositoryViewModel ?? throw new NullReferenceException();

        public override Task Setup()
        {
            _repositoryViewModel = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryViewModel>();
            _repositoryViewModel.VisibleRepositoryList = new List<Repository>();

            return base.Setup();
        }

        [Test]
        public async Task PullToRefreshCommandTest_Authenticated()
        {
            //Arrange
            DateTimeOffset beforePullToRefresh, afterPullToRefresh;
            IReadOnlyList<Repository> visibleRepositoryList_Initial, visibleRepositoryList_Final;

            string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
            string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var graphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            //Act
            await AuthenticateUser(gitHubUserService, graphQLApiService).ConfigureAwait(false);

            beforePullToRefresh = DateTimeOffset.UtcNow;
            emptyDataViewTitle_Initial = RepositoryViewModel.EmptyDataViewTitle;
            visibleRepositoryList_Initial = RepositoryViewModel.VisibleRepositoryList;
            emptyDataViewDescription_Initial = RepositoryViewModel.EmptyDataViewDescription;

            await RepositoryViewModel.PullToRefreshCommand.ExecuteAsync().ConfigureAwait(false);

            afterPullToRefresh = DateTimeOffset.UtcNow;
            emptyDataViewTitle_Final = RepositoryViewModel.EmptyDataViewTitle;
            visibleRepositoryList_Final = RepositoryViewModel.VisibleRepositoryList;
            emptyDataViewDescription_Final = RepositoryViewModel.EmptyDataViewDescription;

            //Assert
            Assert.IsEmpty(visibleRepositoryList_Initial);
            Assert.IsNotEmpty(visibleRepositoryList_Final);

            Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Uninitialized, true), emptyDataViewTitle_Initial);
            Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Succeeded, false), emptyDataViewTitle_Final);

            Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Uninitialized, true), emptyDataViewDescription_Initial);
            Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Succeeded, false), emptyDataViewDescription_Final);

            Assert.IsTrue(visibleRepositoryList_Final.Any(x => x.OwnerLogin is GitTrendsRepoOwner && x.Name is GitTrendsRepoName));

            foreach (var repository in visibleRepositoryList_Final)
            {
                Assert.Less(beforePullToRefresh, repository.DataDownloadedAt);
                Assert.Greater(afterPullToRefresh, repository.DataDownloadedAt);
            }
        }

        [Test]
        public async Task PullToRefreshCommandTest_Unauthenticated()
        {
            //Arrange
            IReadOnlyList<Repository> visibleRepositoryList_Initial, visibleRepositoryList_Final;
            string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;
            string emptyDataViewDescription_Initial, emptyDataViewDescription_Final;

            bool didPullToRefreshFailedFire = false;
            var pullToRefreshFailedTCS = new TaskCompletionSource<PullToRefreshFailedEventArgs>();

            RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

            //Act
            emptyDataViewTitle_Initial = RepositoryViewModel.EmptyDataViewTitle;
            visibleRepositoryList_Initial = RepositoryViewModel.VisibleRepositoryList;
            emptyDataViewDescription_Initial = RepositoryViewModel.EmptyDataViewDescription;

            var pullToRefreshCommandTask = RepositoryViewModel.PullToRefreshCommand.ExecuteAsync();

            await pullToRefreshCommandTask.ConfigureAwait(false);
            var pullToRefreshFailedEventArgs = await pullToRefreshFailedTCS.Task.ConfigureAwait(false);

            emptyDataViewTitle_Final = RepositoryViewModel.EmptyDataViewTitle;
            visibleRepositoryList_Final = RepositoryViewModel.VisibleRepositoryList;
            emptyDataViewDescription_Final = RepositoryViewModel.EmptyDataViewDescription;

            //Assert
            Assert.IsTrue(didPullToRefreshFailedFire);
            Assert.IsInstanceOf(typeof(LoginExpiredPullToRefreshEventArgs), pullToRefreshFailedEventArgs);

            Assert.IsEmpty(visibleRepositoryList_Initial);
            Assert.IsEmpty(visibleRepositoryList_Final);

            Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.Uninitialized, true), emptyDataViewTitle_Initial);
            Assert.AreEqual(EmptyDataViewService.GetRepositoryTitleText(RefreshState.LoginExpired, true), emptyDataViewTitle_Final);

            Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.Uninitialized, true), emptyDataViewDescription_Initial);
            Assert.AreEqual(EmptyDataViewService.GetRepositoryDescriptionText(RefreshState.LoginExpired, true), emptyDataViewDescription_Final);

            void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs e)
            {
                RepositoryViewModel.PullToRefreshFailed -= HandlePullToRefreshFailed;

                didPullToRefreshFailedFire = true;
                pullToRefreshFailedTCS.SetResult(e);
            }
        }

        [Test]
        public async Task FilterRepositoriesCommandTest_InvalidRepositoryName()
        {
            //Arrange
            Repository repository;
            int repositoryListCount_Initial, repositoryListCount_Final;
            IReadOnlyList<Repository> repositoryList_Initial, repositoryList_Final;

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

            //Act
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
            await RepositoryViewModel.PullToRefreshCommand.ExecuteAsync().ConfigureAwait(false);

            repositoryList_Initial = RepositoryViewModel.VisibleRepositoryList;
            repository = repositoryList_Initial.First();
            repositoryListCount_Initial = repositoryList_Initial.Count;

            RepositoryViewModel.FilterRepositoriesCommand.Execute(repository.Name + DemoDataConstants.GetRandomText());

            repositoryList_Final = RepositoryViewModel.VisibleRepositoryList;
            repositoryListCount_Final = repositoryList_Final.Count;

            //Assert
            Assert.IsEmpty(RepositoryViewModel.VisibleRepositoryList);
            Assert.Greater(repositoryListCount_Initial, 0);
            Assert.Less(repositoryListCount_Final, repositoryListCount_Initial);
        }

        [Test]
        public async Task FilterRepositoriesCommandTest_ValidRepositoryName()
        {
            //Arrange
            Repository repository;
            int repositoryListCount_Initial, repositoryListCount_Final;
            IReadOnlyList<Repository> repositoryList_Initial, repositoryList_Final;

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

            //Act
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
            await RepositoryViewModel.PullToRefreshCommand.ExecuteAsync().ConfigureAwait(false);

            repositoryList_Initial = RepositoryViewModel.VisibleRepositoryList;
            repository = repositoryList_Initial.First();
            repositoryListCount_Initial = repositoryList_Initial.Count;

            RepositoryViewModel.FilterRepositoriesCommand.Execute(repository.Name);

            repositoryList_Final = RepositoryViewModel.VisibleRepositoryList;
            repositoryListCount_Final = repositoryList_Final.Count;

            //Assert
            Assert.Contains(repository, RepositoryViewModel.VisibleRepositoryList.ToArray());
            Assert.Greater(repositoryListCount_Initial, 1);
            Assert.Less(repositoryListCount_Final, repositoryListCount_Initial);
        }

        [TestCase((SortingOption)int.MinValue)]
        [TestCase((SortingOption)int.MaxValue)]
        [TestCase((SortingOption)100)]
        [TestCase((SortingOption)(-1))]
        public async Task SortRepositoriesCommandTest_InvalidSortingOption(SortingOption sortingOption)
        {
            //Arrange
            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

            //Act
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
            await RepositoryViewModel.PullToRefreshCommand.ExecuteAsync().ConfigureAwait(false);

            //Assert
            Assert.Throws<InvalidEnumArgumentException>(() => RepositoryViewModel.SortRepositoriesCommand.Execute(sortingOption));
        }

        [TestCase(SortingOption.Clones)]
        [TestCase(SortingOption.Forks)]
        [TestCase(SortingOption.Issues)]
        [TestCase(SortingOption.Stars)]
        [TestCase(SortingOption.UniqueClones)]
        [TestCase(SortingOption.UniqueViews)]
        [TestCase(SortingOption.Views)]
        public async Task SortRepositoriesCommandTest_ValidSortingOption(SortingOption sortingOption)
        {
            //Arrange
            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();

            //Act
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);
            await RepositoryViewModel.PullToRefreshCommand.ExecuteAsync().ConfigureAwait(false);

            RepositoryViewModel.SortRepositoriesCommand.Execute(sortingOption);

            //Assert
            AssertRepositoriesSorted(RepositoryViewModel.VisibleRepositoryList, sortingOption);

            //Act
            RepositoryViewModel.SortRepositoriesCommand.Execute(sortingOption);

            //Assert
            AssertRepositoriesReversedSorted(RepositoryViewModel.VisibleRepositoryList, sortingOption);
        }

        static void AssertRepositoriesReversedSorted(in IEnumerable<Repository> repositories, in SortingOption sortingOption)
        {
            var topRepository = repositories.First();
            var secondTopRepository = repositories.Skip(1).First();

            var lastRepository = repositories.Last();

            switch (sortingOption)
            {
                case SortingOption.Views when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.GreaterOrEqual(topRepository.TotalViews, secondTopRepository.TotalViews);
                    break;
                case SortingOption.Views:
                    Assert.GreaterOrEqual(secondTopRepository.TotalViews, lastRepository.TotalViews);
                    break;
                case SortingOption.Stars when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.LessOrEqual(topRepository.StarCount, secondTopRepository.StarCount);
                    break;
                case SortingOption.Stars:
                    Assert.LessOrEqual(secondTopRepository.StarCount, lastRepository.StarCount);
                    break;
                case SortingOption.Forks when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.LessOrEqual(topRepository.ForkCount, secondTopRepository.ForkCount);
                    break;
                case SortingOption.Forks:
                    Assert.LessOrEqual(secondTopRepository.ForkCount, lastRepository.ForkCount);
                    break;
                case SortingOption.Issues when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.LessOrEqual(topRepository.IssuesCount, secondTopRepository.IssuesCount);
                    break;
                case SortingOption.Issues:
                    Assert.LessOrEqual(secondTopRepository.IssuesCount, lastRepository.IssuesCount);
                    break;
                case SortingOption.Clones when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.LessOrEqual(topRepository.TotalClones, secondTopRepository.TotalClones);
                    break;
                case SortingOption.Clones:
                    Assert.LessOrEqual(secondTopRepository.TotalClones, lastRepository.TotalClones);
                    break;
                case SortingOption.UniqueClones when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.LessOrEqual(topRepository.TotalUniqueClones, secondTopRepository.TotalUniqueClones);
                    break;
                case SortingOption.UniqueClones:
                    Assert.LessOrEqual(secondTopRepository.TotalUniqueClones, lastRepository.TotalUniqueClones);
                    break;
                case SortingOption.UniqueViews when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.LessOrEqual(topRepository.TotalUniqueViews, secondTopRepository.TotalUniqueViews);
                    break;
                case SortingOption.UniqueViews:
                    Assert.LessOrEqual(secondTopRepository.TotalUniqueViews, lastRepository.TotalUniqueViews);
                    break;
                default:
                    throw new NotSupportedException();
            };
        }

        static void AssertRepositoriesSorted(in IEnumerable<Repository> repositories, in SortingOption sortingOption)
        {
            var topRepository = repositories.First();
            var secondTopRepository = repositories.Skip(1).First();

            var lastRepository = repositories.Last();

            switch (sortingOption)
            {
                case SortingOption.Views when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.LessOrEqual(topRepository.TotalViews, secondTopRepository.TotalViews);
                    break;
                case SortingOption.Views:
                    Assert.LessOrEqual(secondTopRepository.TotalViews, lastRepository.TotalViews);
                    break;
                case SortingOption.Stars when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.GreaterOrEqual(topRepository.StarCount, secondTopRepository.StarCount);
                    break;
                case SortingOption.Stars:
                    Assert.GreaterOrEqual(secondTopRepository.StarCount, lastRepository.StarCount);
                    break;
                case SortingOption.Forks when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.GreaterOrEqual(topRepository.ForkCount, secondTopRepository.ForkCount);
                    break;
                case SortingOption.Forks:
                    Assert.GreaterOrEqual(secondTopRepository.ForkCount, lastRepository.ForkCount);
                    break;
                case SortingOption.Issues when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.GreaterOrEqual(topRepository.IssuesCount, secondTopRepository.IssuesCount);
                    break;
                case SortingOption.Issues:
                    Assert.GreaterOrEqual(secondTopRepository.IssuesCount, lastRepository.IssuesCount);
                    break;
                case SortingOption.Clones when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.GreaterOrEqual(topRepository.TotalClones, secondTopRepository.TotalClones);
                    break;
                case SortingOption.Clones:
                    Assert.GreaterOrEqual(secondTopRepository.TotalClones, lastRepository.TotalClones);
                    break;
                case SortingOption.UniqueClones when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.GreaterOrEqual(topRepository.TotalUniqueClones, secondTopRepository.TotalUniqueClones);
                    break;
                case SortingOption.UniqueClones:
                    Assert.GreaterOrEqual(secondTopRepository.TotalUniqueClones, lastRepository.TotalUniqueClones);
                    break;
                case SortingOption.UniqueViews when topRepository.IsTrending == secondTopRepository.IsTrending:
                    Assert.GreaterOrEqual(topRepository.TotalUniqueViews, secondTopRepository.TotalUniqueViews);
                    break;
                case SortingOption.UniqueViews:
                    Assert.GreaterOrEqual(secondTopRepository.TotalUniqueViews, lastRepository.TotalUniqueViews);
                    break;
                default:
                    throw new NotSupportedException();
            };
        }
    }
}
