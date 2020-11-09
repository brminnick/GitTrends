using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class TrendsViewModelTests : BaseTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public async Task FetchDataCommandTest_AuthenticatedUser(bool shouldIncludeViewsClonesData)
        {
            //Arrange
            Repository repository;

            double dailyViewsClonesMinValue_Initial, dailyViewsClonesMinValue_Final;
            double dailyViewsClonesMaxValue_Initial, dailyViewsClonesMaxValue_Final;

            double minDailyStarsValue_Initial, minDailyStarsValue_Final;
            double maxDailyStarsValue_Initial, maxDailyStarsValue_Final;

            string viewsClonesEmptyDataViewTitleText_Initial, viewsClonesEmptyDataViewTitleText_Final;
            string starsEmptyDataViewTitleText_Initial, starsEmptyDataViewTitleText_Final;

            bool isViewsClonesChartVisible_Initial, isViewsClonesChartVisible_DuringFetchDataCommand, isViewsClonesChartVisible_Final;
            bool isStarsChartVisible_Initial, isStarsChartVisible_DuringFetchDataCommand, isStarsChartVisible_Final;

            bool isFetchingData_Initial, isFetchingData_DuringFetchDataCommand, isFetchingData_Final;

            bool isViewsClonesEmptyDataViewVisible_Initial, isViewsClonesEmptyDataViewVisible_DuringFetchDataCommand, isViewsClonesEmptyDataViewVisible_Final;
            bool isStarsEmptyDataViewVisible_Initial, isStarsEmptyDataViewVisible_DuringFetchDataCommand, isStarsEmptyDataViewVisible_Final;

            DateTime minViewsClonesDate_Initial, minViewsClonesDate_Final;
            DateTime maxViewsClonesDate_Initial, maxViewsClonesDate_Final;

            DateTime minDailyStarsDate_Initial, minDailyStarsDate_Final;
            DateTime maxDailyStarsDate_Initial, maxDailyStarsDate_Final;

            IReadOnlyList<DailyStarsModel> dailyStarsList_Initial, dailyStarsList_Final;
            IReadOnlyList<DailyViewsModel> dailyViewsList_Initial, dailyViewsList_Final;
            IReadOnlyList<DailyClonesModel> dailyClonesList_Initial, dailyClonesList_Final;

            var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
            var gitHubApiRepositoriesService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiRepositoriesService>();


            await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

            repository = await gitHubGraphQLApiService.GetRepository(GitTrendsRepoOwner, GitTrendsRepoName, CancellationToken.None);

            if (shouldIncludeViewsClonesData)
            {
                await foreach (var completedReposiory in gitHubApiRepositoriesService.UpdateRepositoriesWithViewsClonesAndStarsData(new[] { repository }, CancellationToken.None))
                {
                    repository = completedReposiory;
                }
            }

            //Act
            isFetchingData_Initial = trendsViewModel.IsFetchingData;
            dailyStarsList_Initial = trendsViewModel.DailyStarsList;
            dailyViewsList_Initial = trendsViewModel.DailyViewsList;
            dailyClonesList_Initial = trendsViewModel.DailyClonesList;
            maxDailyStarsDate_Initial = trendsViewModel.MaxDailyStarsDate;
            minDailyStarsDate_Initial = trendsViewModel.MinDailyStarsDate;
            minDailyStarsValue_Initial = trendsViewModel.MinDailyStarsValue;
            maxDailyStarsValue_Initial = trendsViewModel.MaxDailyStarsValue;
            minViewsClonesDate_Initial = trendsViewModel.MinViewsClonesDate;
            maxViewsClonesDate_Initial = trendsViewModel.MaxViewsClonesDate;
            isStarsChartVisible_Initial = trendsViewModel.IsStarsChartVisible;
            dailyViewsClonesMinValue_Initial = trendsViewModel.DailyViewsClonesMinValue;
            dailyViewsClonesMaxValue_Initial = trendsViewModel.DailyViewsClonesMaxValue;
            isViewsClonesChartVisible_Initial = trendsViewModel.IsViewsClonesChartVisible;
            starsEmptyDataViewTitleText_Initial = trendsViewModel.StarsEmptyDataViewTitleText;
            isStarsEmptyDataViewVisible_Initial = trendsViewModel.IsStarsEmptyDataViewVisible;
            viewsClonesEmptyDataViewTitleText_Initial = trendsViewModel.ViewsClonesEmptyDataViewTitleText;
            isViewsClonesEmptyDataViewVisible_Initial = trendsViewModel.IsViewsClonesEmptyDataViewVisible;

            var fetchDataCommandTask = trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None));

            isFetchingData_DuringFetchDataCommand = trendsViewModel.IsFetchingData;
            isStarsChartVisible_DuringFetchDataCommand = trendsViewModel.IsStarsChartVisible;
            isViewsClonesChartVisible_DuringFetchDataCommand = trendsViewModel.IsViewsClonesChartVisible;
            isStarsEmptyDataViewVisible_DuringFetchDataCommand = trendsViewModel.IsStarsEmptyDataViewVisible;
            isViewsClonesEmptyDataViewVisible_DuringFetchDataCommand = trendsViewModel.IsViewsClonesEmptyDataViewVisible;

            await fetchDataCommandTask.ConfigureAwait(false);

            isFetchingData_Final = trendsViewModel.IsFetchingData;
            dailyStarsList_Final = trendsViewModel.DailyStarsList;
            dailyViewsList_Final = trendsViewModel.DailyViewsList;
            dailyClonesList_Final = trendsViewModel.DailyClonesList;
            maxDailyStarsDate_Final = trendsViewModel.MaxDailyStarsDate;
            minDailyStarsDate_Final = trendsViewModel.MinDailyStarsDate;
            minDailyStarsValue_Final = trendsViewModel.MinDailyStarsValue;
            maxDailyStarsValue_Final = trendsViewModel.MaxDailyStarsValue;
            isStarsChartVisible_Final = trendsViewModel.IsStarsChartVisible;
            minViewsClonesDate_Final = trendsViewModel.MinViewsClonesDate;
            maxViewsClonesDate_Final = trendsViewModel.MaxViewsClonesDate;
            dailyViewsClonesMinValue_Final = trendsViewModel.DailyViewsClonesMinValue;
            dailyViewsClonesMaxValue_Final = trendsViewModel.DailyViewsClonesMaxValue;
            isViewsClonesChartVisible_Final = trendsViewModel.IsViewsClonesChartVisible;
            starsEmptyDataViewTitleText_Final = trendsViewModel.StarsEmptyDataViewTitleText;
            isStarsEmptyDataViewVisible_Final = trendsViewModel.IsStarsEmptyDataViewVisible;
            viewsClonesEmptyDataViewTitleText_Final = trendsViewModel.ViewsClonesEmptyDataViewTitleText;
            isViewsClonesEmptyDataViewVisible_Final = trendsViewModel.IsViewsClonesEmptyDataViewVisible;

            //Assert
            Assert.IsEmpty(dailyViewsList_Initial);
            Assert.IsEmpty(dailyClonesList_Initial);
            Assert.IsEmpty(dailyStarsList_Initial);

            Assert.IsNotEmpty(dailyViewsList_Final);
            Assert.IsNotEmpty(dailyClonesList_Final);
            Assert.IsNotEmpty(dailyStarsList_Final);

            Assert.IsTrue(isFetchingData_Initial);
            Assert.IsTrue(isFetchingData_DuringFetchDataCommand);
            Assert.IsFalse(isFetchingData_Final);

            Assert.IsFalse(isViewsClonesEmptyDataViewVisible_Initial);
            Assert.IsFalse(isViewsClonesEmptyDataViewVisible_DuringFetchDataCommand);
            Assert.IsFalse(isViewsClonesEmptyDataViewVisible_Final);

            Assert.IsFalse(isStarsEmptyDataViewVisible_Initial);
            Assert.IsFalse(isStarsEmptyDataViewVisible_DuringFetchDataCommand);
            Assert.IsFalse(isStarsEmptyDataViewVisible_Final);

            Assert.IsFalse(isViewsClonesChartVisible_Initial);
            Assert.IsFalse(isViewsClonesChartVisible_DuringFetchDataCommand);
            Assert.IsTrue(isViewsClonesChartVisible_Final);

            Assert.IsFalse(isStarsChartVisible_Initial);
            Assert.IsFalse(isStarsChartVisible_DuringFetchDataCommand);
            Assert.IsTrue(isStarsChartVisible_Final);

            Assert.AreEqual(TrendsViewModel.MinumumChartHeight, dailyViewsClonesMaxValue_Initial);
            Assert.Greater(dailyViewsClonesMaxValue_Final, dailyViewsClonesMaxValue_Initial);

            Assert.AreEqual(TrendsViewModel.MinumumChartHeight, maxDailyStarsValue_Initial);
            Assert.Greater(maxDailyStarsValue_Final, maxDailyStarsValue_Initial);

            Assert.AreEqual(0, dailyViewsClonesMinValue_Initial);
            Assert.AreEqual(dailyViewsClonesMinValue_Final, dailyViewsClonesMinValue_Initial);

            Assert.AreEqual(0, minDailyStarsValue_Initial);
            Assert.AreEqual(minDailyStarsValue_Final, minDailyStarsValue_Initial);

            Assert.AreEqual(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13)).ToLocalTime().Date, minViewsClonesDate_Initial.ToLocalTime().Date);
            Assert.LessOrEqual(minViewsClonesDate_Final.ToLocalTime().Date, minViewsClonesDate_Initial.ToLocalTime().Date);

            Assert.AreEqual(DateTimeOffset.UtcNow.ToLocalTime().Date, maxViewsClonesDate_Initial.ToLocalTime().Date);
            Assert.LessOrEqual(maxViewsClonesDate_Final.ToLocalTime().Date, maxViewsClonesDate_Initial.ToLocalTime().Date);

            Assert.LessOrEqual(minDailyStarsDate_Final.ToLocalTime().Date, minDailyStarsDate_Initial.ToLocalTime().Date);
            Assert.LessOrEqual(maxDailyStarsDate_Final.ToLocalTime().Date, maxDailyStarsDate_Initial.ToLocalTime().Date);

            Assert.AreEqual(EmptyDataViewService.GetViewsClonesTitleText(RefreshState.Uninitialized), viewsClonesEmptyDataViewTitleText_Initial);
            Assert.AreEqual(EmptyDataViewService.GetViewsClonesTitleText(RefreshState.Succeeded), viewsClonesEmptyDataViewTitleText_Final);

            Assert.AreEqual(EmptyDataViewService.GetStarsTitleText(RefreshState.Uninitialized, trendsViewModel.TotalStars), starsEmptyDataViewTitleText_Initial);
            Assert.AreEqual(EmptyDataViewService.GetStarsTitleText(RefreshState.Succeeded, trendsViewModel.TotalStars), starsEmptyDataViewTitleText_Final);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task FetchDataCommandTest_DemoUser(bool shouldCreateViewsClonesData)
        {
            //Arrange
            IReadOnlyList<DailyStarsModel> dailyStarsList_Initial, dailyStarsList_Final;
            IReadOnlyList<DailyViewsModel> dailyViewsList_Initial, dailyViewsList_Final;
            IReadOnlyList<DailyClonesModel> dailyClonesList_Initial, dailyClonesList_Final;

            Repository repository = CreateRepository(shouldCreateViewsClonesData);

            var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

            //Act
            dailyStarsList_Initial = trendsViewModel.DailyStarsList;
            dailyViewsList_Initial = trendsViewModel.DailyViewsList;
            dailyClonesList_Initial = trendsViewModel.DailyClonesList;

            await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

            dailyStarsList_Final = trendsViewModel.DailyStarsList;
            dailyViewsList_Final = trendsViewModel.DailyViewsList;
            dailyClonesList_Final = trendsViewModel.DailyClonesList;

            //Assret
            Assert.IsEmpty(dailyStarsList_Initial);
            Assert.IsEmpty(dailyViewsList_Initial);
            Assert.IsEmpty(dailyClonesList_Initial);

            Assert.IsNotEmpty(dailyStarsList_Final);
            Assert.IsNotEmpty(dailyViewsList_Final);
            Assert.IsNotEmpty(dailyClonesList_Final);
        }

        [Test]
        public async Task UniqueClonesCardTappedCommandTest()
        {
            //Arrange
            bool isUniqueClonesSeriesVisible_Initial, isUniqueClonesSeriesVisible_AfterFetchDataCommand, isUniqueClonesSeriesVisible_AfterViewsCardTappedCommand;
            string uniqueClonesStatisticsText_Initial, uniqueClonesStatisticsText_AfterFetchDataCommand, uniqueClonesStatisticsText_AfterViewsCardTappedCommand;

            Repository repository = CreateRepository();

            var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

            //Act
            isUniqueClonesSeriesVisible_Initial = trendsViewModel.IsUniqueClonesSeriesVisible;
            uniqueClonesStatisticsText_Initial = trendsViewModel.UniqueClonesStatisticsText;

            await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

            isUniqueClonesSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsUniqueClonesSeriesVisible;
            uniqueClonesStatisticsText_AfterFetchDataCommand = trendsViewModel.UniqueClonesStatisticsText;

            trendsViewModel.UniqueClonesCardTappedCommand.Execute(null);

            isUniqueClonesSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsUniqueClonesSeriesVisible;
            uniqueClonesStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.UniqueClonesStatisticsText;

            //Assert
            Assert.AreEqual(string.Empty, uniqueClonesStatisticsText_Initial);
            Assert.AreEqual(repository.TotalUniqueClones.ToAbbreviatedText(), uniqueClonesStatisticsText_AfterFetchDataCommand);
            Assert.AreEqual(repository.TotalUniqueClones.ToAbbreviatedText(), uniqueClonesStatisticsText_AfterViewsCardTappedCommand);

            Assert.IsTrue(isUniqueClonesSeriesVisible_Initial);
            Assert.IsTrue(isUniqueClonesSeriesVisible_AfterFetchDataCommand);
            Assert.IsFalse(isUniqueClonesSeriesVisible_AfterViewsCardTappedCommand);
        }

        [Test]
        public async Task ClonesCardTappedCommandTest()
        {
            //Arrange
            bool isClonesSeriesVisible_Initial, isClonesSeriesVisible_AfterFetchDataCommand, isClonesSeriesVisible_AfterViewsCardTappedCommand;
            string clonesStatisticsText_Initial, clonesStatisticsText_AfterFetchDataCommand, clonesStatisticsText_AfterViewsCardTappedCommand;

            Repository repository = CreateRepository();

            var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

            //Act
            isClonesSeriesVisible_Initial = trendsViewModel.IsClonesSeriesVisible;
            clonesStatisticsText_Initial = trendsViewModel.ClonesStatisticsText;

            await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

            isClonesSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsClonesSeriesVisible;
            clonesStatisticsText_AfterFetchDataCommand = trendsViewModel.ClonesStatisticsText;

            trendsViewModel.ClonesCardTappedCommand.Execute(null);

            isClonesSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsClonesSeriesVisible;
            clonesStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.ClonesStatisticsText;

            //Assert
            Assert.AreEqual(string.Empty, clonesStatisticsText_Initial);
            Assert.AreEqual(repository.TotalClones.ToAbbreviatedText(), clonesStatisticsText_AfterFetchDataCommand);
            Assert.AreEqual(repository.TotalClones.ToAbbreviatedText(), clonesStatisticsText_AfterViewsCardTappedCommand);

            Assert.IsTrue(isClonesSeriesVisible_Initial);
            Assert.IsTrue(isClonesSeriesVisible_AfterFetchDataCommand);
            Assert.IsFalse(isClonesSeriesVisible_AfterViewsCardTappedCommand);
        }


        [Test]
        public async Task UniqueViewsCardTappedCommandTest()
        {
            //Arrange
            bool isUniqueViewsSeriesVisible_Initial, isUniqueViewsSeriesVisible_AfterFetchDataCommand, isUniqueViewsSeriesVisible_AfterViewsCardTappedCommand;
            string uniqueViewsStatisticsText_Initial, uniqueViewsStatisticsText_AfterFetchDataCommand, uniqueViewsStatisticsText_AfterViewsCardTappedCommand;

            Repository repository = CreateRepository();

            var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

            //Act
            isUniqueViewsSeriesVisible_Initial = trendsViewModel.IsUniqueViewsSeriesVisible;
            uniqueViewsStatisticsText_Initial = trendsViewModel.UniqueViewsStatisticsText;

            await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

            isUniqueViewsSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsUniqueViewsSeriesVisible;
            uniqueViewsStatisticsText_AfterFetchDataCommand = trendsViewModel.UniqueViewsStatisticsText;

            trendsViewModel.UniqueViewsCardTappedCommand.Execute(null);

            isUniqueViewsSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsUniqueViewsSeriesVisible;
            uniqueViewsStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.UniqueViewsStatisticsText;

            //Assert
            Assert.AreEqual(string.Empty, uniqueViewsStatisticsText_Initial);
            Assert.AreEqual(repository.TotalUniqueViews.ToAbbreviatedText(), uniqueViewsStatisticsText_AfterFetchDataCommand);
            Assert.AreEqual(repository.TotalUniqueViews.ToAbbreviatedText(), uniqueViewsStatisticsText_AfterViewsCardTappedCommand);

            Assert.IsTrue(isUniqueViewsSeriesVisible_Initial);
            Assert.IsTrue(isUniqueViewsSeriesVisible_AfterFetchDataCommand);
            Assert.IsFalse(isUniqueViewsSeriesVisible_AfterViewsCardTappedCommand);
        }

        [Test]
        public async Task ViewsCardTappedCommandTest()
        {
            //Arrange
            bool isViewsSeriesVisible_Initial, isViewsSeriesVisible_AfterFetchDataCommand, isViewsSeriesVisible_AfterViewsCardTappedCommand;
            string viewsStatisticsText_Initial, viewsStatisticsText_AfterFetchDataCommand, viewsStatisticsText_AfterViewsCardTappedCommand;

            Repository repository = CreateRepository();

            var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

            //Act
            isViewsSeriesVisible_Initial = trendsViewModel.IsViewsSeriesVisible;
            viewsStatisticsText_Initial = trendsViewModel.ViewsStatisticsText;

            await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

            isViewsSeriesVisible_AfterFetchDataCommand = trendsViewModel.IsViewsSeriesVisible;
            viewsStatisticsText_AfterFetchDataCommand = trendsViewModel.ViewsStatisticsText;

            trendsViewModel.ViewsCardTappedCommand.Execute(null);

            isViewsSeriesVisible_AfterViewsCardTappedCommand = trendsViewModel.IsViewsSeriesVisible;
            viewsStatisticsText_AfterViewsCardTappedCommand = trendsViewModel.ViewsStatisticsText;

            //Assert
            Assert.AreEqual(string.Empty, viewsStatisticsText_Initial);
            Assert.AreEqual(repository.TotalViews.ToAbbreviatedText(), viewsStatisticsText_AfterFetchDataCommand);
            Assert.AreEqual(repository.TotalViews.ToAbbreviatedText(), viewsStatisticsText_AfterViewsCardTappedCommand);

            Assert.IsTrue(isViewsSeriesVisible_Initial);
            Assert.IsTrue(isViewsSeriesVisible_AfterFetchDataCommand);
            Assert.IsFalse(isViewsSeriesVisible_AfterViewsCardTappedCommand);
        }
    }
}
