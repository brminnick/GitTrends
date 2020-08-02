using System;
using System.Collections.Generic;
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

            string emptyDataViewTitle_Initial, emptyDataViewTitle_Final;

            bool isChartVisible_Initial, isChartVisible_DuringFetchDataCommand, isChartVisible_Final;
            bool isFetchingData_Initial, isFetchingData_DuringFetchDataCommand, isFetchingData_Final;
            bool isEmptyDataViewVisible_Initial, isEmptyDataViewVisible_DuringFetchDataCommand, isEmptyDataViewVisible_Final;

            DateTime minDateValue_Initial, minDateValue_Final;
            DateTime maxDateValue_Initial, maxDateValue_Final;

            IReadOnlyList<DailyViewsModel> dailyViewsModels_Initial, dailyViewsModels_Final;
            IReadOnlyList<DailyClonesModel> dailyClonesModels_Initial, dailyClonesModels_Final;

            var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

            var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
            var gitHubApiV3Service = ServiceCollection.ServiceProvider.GetRequiredService<GitHubApiV3Service>();

            await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService).ConfigureAwait(false);

            repository = await gitHubGraphQLApiService.GetRepository(GitTrendsRepoOwner, GitTrendsRepoName, CancellationToken.None);

            if (shouldIncludeViewsClonesData)
            {
                await foreach (var completedReposiory in gitHubApiV3Service.UpdateRepositoriesWithViewsAndClonesData(new[] { repository }, CancellationToken.None))
                {
                    repository = completedReposiory;
                }
            }

            //Act
            minDateValue_Initial = trendsViewModel.MinDateValue;
            maxDateValue_Initial = trendsViewModel.MaxDateValue;
            isFetchingData_Initial = trendsViewModel.IsFetchingData;
            isChartVisible_Initial = trendsViewModel.IsChartVisible;
            dailyViewsModels_Initial = trendsViewModel.DailyViewsList;
            dailyClonesModels_Initial = trendsViewModel.DailyClonesList;
            emptyDataViewTitle_Initial = trendsViewModel.EmptyDataViewTitle;
            isEmptyDataViewVisible_Initial = trendsViewModel.IsEmptyDataViewVisible;
            dailyViewsClonesMinValue_Initial = trendsViewModel.DailyViewsClonesMinValue;
            dailyViewsClonesMaxValue_Initial = trendsViewModel.DailyViewsClonesMaxValue;

            var fetchDataCommandTask = trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None));

            isFetchingData_DuringFetchDataCommand = trendsViewModel.IsFetchingData;
            isChartVisible_DuringFetchDataCommand = trendsViewModel.IsChartVisible;
            isEmptyDataViewVisible_DuringFetchDataCommand = trendsViewModel.IsEmptyDataViewVisible;

            await fetchDataCommandTask.ConfigureAwait(false);

            minDateValue_Final = trendsViewModel.MinDateValue;
            maxDateValue_Final = trendsViewModel.MaxDateValue;
            isFetchingData_Final = trendsViewModel.IsFetchingData;
            isChartVisible_Final = trendsViewModel.IsChartVisible;
            dailyViewsModels_Final = trendsViewModel.DailyViewsList;
            dailyClonesModels_Final = trendsViewModel.DailyClonesList;
            emptyDataViewTitle_Final = trendsViewModel.EmptyDataViewTitle;
            isEmptyDataViewVisible_Final = trendsViewModel.IsEmptyDataViewVisible;
            dailyViewsClonesMinValue_Final = trendsViewModel.DailyViewsClonesMinValue;
            dailyViewsClonesMaxValue_Final = trendsViewModel.DailyViewsClonesMaxValue;

            //Assert
            Assert.IsEmpty(dailyViewsModels_Initial);
            Assert.IsEmpty(dailyClonesModels_Initial);

            Assert.IsNotEmpty(dailyViewsModels_Final);
            Assert.IsNotEmpty(dailyClonesModels_Final);

            Assert.IsTrue(isFetchingData_Initial);
            Assert.IsTrue(isFetchingData_DuringFetchDataCommand);
            Assert.IsFalse(isFetchingData_Final);

            Assert.IsFalse(isEmptyDataViewVisible_Initial);
            Assert.IsFalse(isEmptyDataViewVisible_DuringFetchDataCommand);
            Assert.IsFalse(isEmptyDataViewVisible_Final);

            Assert.IsFalse(isChartVisible_Initial);
            Assert.IsFalse(isChartVisible_DuringFetchDataCommand);
            Assert.IsTrue(isChartVisible_Final);

            Assert.AreEqual(TrendsViewModel.MinumumChartHeight, dailyViewsClonesMaxValue_Initial);
            Assert.Greater(dailyViewsClonesMaxValue_Final, dailyViewsClonesMaxValue_Initial);

            Assert.AreEqual(0, dailyViewsClonesMinValue_Initial);
            Assert.AreEqual(dailyViewsClonesMinValue_Final, dailyViewsClonesMinValue_Initial);

            Assert.AreEqual(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13)).ToLocalTime().Date, minDateValue_Initial.ToLocalTime().Date);
            Assert.LessOrEqual(minDateValue_Final.ToLocalTime().Date, minDateValue_Initial.ToLocalTime().Date);

            Assert.AreEqual(DateTimeOffset.UtcNow.ToLocalTime().Date, maxDateValue_Initial.ToLocalTime().Date);
            Assert.LessOrEqual(maxDateValue_Final.ToLocalTime().Date, maxDateValue_Initial.ToLocalTime().Date);

            Assert.AreEqual(string.Empty, emptyDataViewTitle_Initial);
            Assert.AreEqual(EmptyDataViewConstants.NoTrafficYet, emptyDataViewTitle_Final);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task FetchDataCommandTest_DemoUser(bool shouldCreateViewsClonesData)
        {
            //Arrange
            IReadOnlyList<DailyViewsModel> dailyViewsModels_Initial, dailyViewsModels_Final;
            IReadOnlyList<DailyClonesModel> dailyClonesModels_Initial, dailyClonesModels_Final;

            Repository repository = CreateRepository(shouldCreateViewsClonesData);

            var trendsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<TrendsViewModel>();

            var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
            await gitHubAuthenticationService.ActivateDemoUser().ConfigureAwait(false);

            //Act
            dailyViewsModels_Initial = trendsViewModel.DailyViewsList;
            dailyClonesModels_Initial = trendsViewModel.DailyClonesList;

            await trendsViewModel.FetchDataCommand.ExecuteAsync((repository, CancellationToken.None)).ConfigureAwait(false);

            dailyViewsModels_Final = trendsViewModel.DailyViewsList;
            dailyClonesModels_Final = trendsViewModel.DailyClonesList;

            //Assret
            Assert.IsEmpty(dailyViewsModels_Initial);
            Assert.IsEmpty(dailyClonesModels_Initial);

            Assert.IsNotEmpty(dailyViewsModels_Final);
            Assert.IsNotEmpty(dailyClonesModels_Final);
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
