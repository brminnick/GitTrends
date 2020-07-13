using System;
using System.Collections.Generic;
using System.Linq;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class MobileSortingServiceTests : BaseTest
    {
        [Test]
        public void SortReferringSitesTests()
        {
            const int largestTotalCount = 1000;
            const int largestTotalUniqueCount = 11;
            const string lastReferer = "t.co";

            //Arrange
            var referringSitesList = new List<MobileReferringSiteModel>
            {
                new MobileReferringSiteModel(new ReferringSiteModel(10, 10, "Google")),
                new MobileReferringSiteModel(new ReferringSiteModel(10, 10, "codetraver.io")),
                new MobileReferringSiteModel(new ReferringSiteModel(10, 10, lastReferer)),
                new MobileReferringSiteModel(new ReferringSiteModel(100, largestTotalUniqueCount, "facebook.com")),
                new MobileReferringSiteModel(new ReferringSiteModel(100, 9, "linkedin.com")),
                new MobileReferringSiteModel(new ReferringSiteModel(largestTotalCount, 9, "reddit.com"))
            };

            //Act
            var sortedReferringSitesList = MobileSortingService.SortReferringSites(referringSitesList);

            //Assert
            Assert.IsTrue(sortedReferringSitesList.First().TotalCount is largestTotalCount);
            Assert.IsTrue(sortedReferringSitesList.Skip(1).First().TotalUniqueCount is largestTotalUniqueCount);
            Assert.IsTrue(sortedReferringSitesList.Last().Referrer is lastReferer);
        }

        [TestCase(MobileSortingService.DefaultSortingOption, true)]
        [TestCase(SortingOption.Clones, true)]
        [TestCase(SortingOption.Forks, true)]
        [TestCase(SortingOption.Issues, true)]
        [TestCase(SortingOption.Stars, true)]
        [TestCase(SortingOption.UniqueClones, true)]
        [TestCase(SortingOption.UniqueViews, true)]
        [TestCase(SortingOption.Views, true)]
        [TestCase(MobileSortingService.DefaultSortingOption, false)]
        [TestCase(SortingOption.Clones, false)]
        [TestCase(SortingOption.Forks, false)]
        [TestCase(SortingOption.Issues, false)]
        [TestCase(SortingOption.Stars, false)]
        [TestCase(SortingOption.UniqueClones, false)]
        [TestCase(SortingOption.UniqueViews, false)]
        [TestCase(SortingOption.Views, false)]
        public void SortRepositoriesTests(SortingOption sortingOption, bool isReversed)
        {
            //Arrange
            Repository topRepository, bottomRepository;

            List<Repository> repositoryList = new List<Repository>();
            for (int i = 0; i < DemoDataConstants.RepoCount; i++)
            {
                repositoryList.Add(CreateRepository());
            }

            //Act
            var sortedRepositoryList = MobileSortingService.SortRepositories(repositoryList, sortingOption, isReversed);
            topRepository = sortedRepositoryList.First();
            bottomRepository = sortedRepositoryList.Last();

            //Assert
            switch (sortingOption)
            {
                case SortingOption.Clones when isReversed:
                    Assert.Less(topRepository.TotalClones, bottomRepository.TotalClones);
                    break;
                case SortingOption.Clones:
                    Assert.Greater(topRepository.TotalClones, bottomRepository.TotalClones);
                    break;
                case SortingOption.Forks when isReversed:
                    Assert.Less(topRepository.ForkCount, bottomRepository.ForkCount);
                    break;
                case SortingOption.Forks:
                    Assert.Greater(topRepository.ForkCount, bottomRepository.ForkCount);
                    break;
                case SortingOption.Issues when isReversed:
                    Assert.Less(topRepository.IssuesCount, bottomRepository.IssuesCount);
                    break;
                case SortingOption.Issues:
                    Assert.Greater(topRepository.IssuesCount, bottomRepository.IssuesCount);
                    break;
                case SortingOption.Stars when isReversed:
                    Assert.Less(topRepository.StarCount, bottomRepository.StarCount);
                    break;
                case SortingOption.Stars:
                    Assert.Greater(topRepository.StarCount, bottomRepository.StarCount);
                    break;
                case SortingOption.UniqueClones when isReversed:
                    Assert.Less(topRepository.TotalUniqueClones, bottomRepository.TotalUniqueClones);
                    break;
                case SortingOption.UniqueClones:
                    Assert.Greater(topRepository.TotalUniqueClones, bottomRepository.TotalUniqueClones);
                    break;
                case SortingOption.UniqueViews when isReversed:
                    Assert.Less(topRepository.TotalUniqueViews, bottomRepository.TotalUniqueViews);
                    break;
                case SortingOption.UniqueViews:
                    Assert.Greater(topRepository.TotalUniqueViews, bottomRepository.TotalUniqueViews);
                    break;
                case SortingOption.Views when isReversed:
                    Assert.Less(topRepository.TotalViews, bottomRepository.TotalViews);
                    break;
                case SortingOption.Views:
                    Assert.Greater(topRepository.TotalViews, bottomRepository.TotalViews);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        [Test]
        public void IsReversedTest()
        {
            //Arrange
            bool isReversed_Initial, isReversed_AfterTrue, isReversed_AfterFalse;
            var sortingService = ServiceCollection.ServiceProvider.GetRequiredService<MobileSortingService>();

            //Act
            isReversed_Initial = sortingService.IsReversed;

            sortingService.IsReversed = true;
            isReversed_AfterTrue = sortingService.IsReversed;

            sortingService.IsReversed = false;
            isReversed_AfterFalse = sortingService.IsReversed;

            //Assert
            Assert.IsFalse(isReversed_Initial);
            Assert.IsTrue(isReversed_AfterTrue);
            Assert.IsFalse(isReversed_AfterFalse);
        }

        [TestCase(SortingOption.Clones, SortingCategory.Clones)]
        [TestCase(SortingOption.Forks, SortingCategory.IssuesForks)]
        [TestCase(SortingOption.Issues, SortingCategory.IssuesForks)]
        [TestCase(SortingOption.Stars, SortingCategory.Views)]
        [TestCase(SortingOption.UniqueClones, SortingCategory.Clones)]
        [TestCase(SortingOption.UniqueViews, SortingCategory.Views)]
        [TestCase(SortingOption.Views, SortingCategory.Views)]
        public void GetSortingCategoryTest_ValidOption(SortingOption sortingOption, SortingCategory expectedSortingCategory)
        {
            //Arrange
            SortingCategory actualSortingCategory;

            //Act
            actualSortingCategory = MobileSortingService.GetSortingCategory(sortingOption);

            //Assert
            Assert.AreEqual(expectedSortingCategory, actualSortingCategory);
        }

        [TestCase(int.MinValue)]
        [TestCase(-1)]
        [TestCase(7)]
        [TestCase(int.MaxValue)]
        public void GetSortingCategoryTest_InvalidOption(SortingOption sortingOption)
        {
            //Arrange

            //Act //Assert
            Assert.Throws<NotSupportedException>(() => MobileSortingService.GetSortingCategory(sortingOption));
        }

        [TestCase(SortingOption.Clones)]
        [TestCase(SortingOption.Forks)]
        [TestCase(SortingOption.Issues)]
        [TestCase(SortingOption.Stars)]
        [TestCase(SortingOption.UniqueClones)]
        [TestCase(SortingOption.UniqueViews)]
        [TestCase(SortingOption.Views)]
        public void CurrentOptionTest_ValidOption(SortingOption sortingOption)
        {
            //Arrange
            SortingOption currentOption_Initial, currentOption_Final;

            var sortingService = ServiceCollection.ServiceProvider.GetRequiredService<MobileSortingService>();

            //Act
            currentOption_Initial = sortingService.CurrentOption;

            sortingService.CurrentOption = sortingOption;
            currentOption_Final = sortingService.CurrentOption;

            //Assert
            Assert.AreEqual(MobileSortingService.DefaultSortingOption, currentOption_Initial);
            Assert.AreEqual(sortingOption, currentOption_Final);
        }

        public void CurrentOptionTest_InvalidOption()
        {
            //Arrange
            SortingOption currentOption_Initial, currentOption_PlusOne, currentOption_NegativeOne;

            var sortingService = ServiceCollection.ServiceProvider.GetRequiredService<MobileSortingService>();

            //Act
            currentOption_Initial = sortingService.CurrentOption;

            sortingService.CurrentOption = (SortingOption)(Enum.GetNames(typeof(SortingOption)).Count() + 1);
            currentOption_PlusOne = sortingService.CurrentOption;

            sortingService.CurrentOption = (SortingOption)(-1);
            currentOption_NegativeOne = sortingService.CurrentOption;

            //Assert
            Assert.AreEqual(MobileSortingService.DefaultSortingOption, currentOption_Initial);
            Assert.AreEqual(MobileSortingService.DefaultSortingOption, currentOption_PlusOne);
            Assert.AreEqual(MobileSortingService.DefaultSortingOption, currentOption_NegativeOne);
        }
    }
}