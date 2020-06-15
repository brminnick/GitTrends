using System;
using GitTrends.Mobile.Common;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class EmptyDataViewServiceTests : BaseTest
    {
        [TestCase(RefreshState.Uninitialized, "Data not gathered")]
        [TestCase(RefreshState.Succeeded, "No referrals yet")]
        [TestCase(RefreshState.LoginExpired, "GitHub Login Expired")]
        [TestCase(RefreshState.Error, "Unable to retrieve data")]
        [TestCase(RefreshState.MaximumApiLimit, "Unable to retrieve data")]
        public void GetReferringSitesTitleTextTest_ValidRefreshState(RefreshState refreshState, string expectedResult)
        {
            //Arrange
            string actualResult;

            //Act
            actualResult = EmptyDataViewService.GetReferringSitesTitleText(refreshState);

            //Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestCase(int.MinValue)]
        [TestCase(-1)]
        [TestCase(5)]
        [TestCase(int.MaxValue)]
        public void GetReferringSitesTitleTextTest_InvalidRefreshState(RefreshState refreshState)
        {
            //Arrange

            //Act

            //Assert
            Assert.Throws<NotSupportedException>(() => EmptyDataViewService.GetReferringSitesTitleText(refreshState));
        }

        [TestCase(RefreshState.Uninitialized, "Swipe down to retrieve referring sites")]
        [TestCase(RefreshState.Succeeded, "")]
        [TestCase(RefreshState.LoginExpired, "Please login again")]
        [TestCase(RefreshState.Error, "Swipe down to retrieve referring sites")]
        [TestCase(RefreshState.MaximumApiLimit, "Swipe down to retrieve referring sites")]
        public void GetReferringSitesDescriptionText_ValidRefreshState(RefreshState refreshState, string expectedResult)
        {
            //Arrange
            string actualResult;

            //Act
            actualResult = EmptyDataViewService.GetReferringSitesDescriptionText(refreshState);

            //Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestCase(int.MinValue)]
        [TestCase(-1)]
        [TestCase(5)]
        [TestCase(int.MaxValue)]
        public void GetReferringSitesTitleDescriptionTest_InvalidRefreshState(RefreshState refreshState)
        {
            //Arrange

            //Act

            //Assert
            Assert.Throws<NotSupportedException>(() => EmptyDataViewService.GetReferringSitesDescriptionText(refreshState));
        }

        [TestCase(RefreshState.Uninitialized, true, "Data not gathered")]
        [TestCase(RefreshState.Uninitialized, false, "Data not gathered")]
        [TestCase(RefreshState.Succeeded, true, "Your repositories list is empty")]
        [TestCase(RefreshState.Succeeded, false, "No Matching Repository Found")]
        [TestCase(RefreshState.LoginExpired, true, "GitHub Login Expired")]
        [TestCase(RefreshState.LoginExpired, false, "GitHub Login Expired")]
        [TestCase(RefreshState.Error, true, "Unable to retrieve data")]
        [TestCase(RefreshState.Error, false, "No Matching Repository Found")]
        [TestCase(RefreshState.MaximumApiLimit, true, "Unable to retrieve data")]
        [TestCase(RefreshState.MaximumApiLimit, false, "Unable to retrieve data")]
        public void GetRepositoresTitleTextTest_ValidRefreshState(RefreshState refreshState, bool isRepositoryListEmpty, string expectedResult)
        {
            //Arrange
            string actualResult;

            //Act
            actualResult = EmptyDataViewService.GetRepositoryTitleText(refreshState, isRepositoryListEmpty);

            //Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestCase(int.MinValue, true)]
        [TestCase(int.MinValue, false)]
        [TestCase(-1, true)]
        [TestCase(-1, false)]
        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(int.MaxValue, true)]
        [TestCase(int.MaxValue, false)]
        public void GetRepositoryTitleTextTest_InvalidRefreshState(RefreshState refreshState, bool isRepositoryListEmpty)
        {
            //Arrange

            //Act

            //Assert
            Assert.Throws<NotSupportedException>(() => EmptyDataViewService.GetRepositoryTitleText(refreshState, isRepositoryListEmpty));
        }

        [TestCase(RefreshState.Uninitialized, true, "Swipe down to retrieve repositories")]
        [TestCase(RefreshState.Uninitialized, false, "Swipe down to retrieve repositories")]
        [TestCase(RefreshState.Succeeded, true, "")]
        [TestCase(RefreshState.Succeeded, false, "Clear search bar and try again")]
        [TestCase(RefreshState.LoginExpired, true, "Please login again")]
        [TestCase(RefreshState.LoginExpired, false, "Please login again")]
        [TestCase(RefreshState.Error, true, "Swipe down to retrieve repositories")]
        [TestCase(RefreshState.Error, false, "Clear search bar and try again")]
        [TestCase(RefreshState.MaximumApiLimit, true, "Swipe down to retrieve repositories")]
        [TestCase(RefreshState.MaximumApiLimit, false, "Swipe down to retrieve repositories")]
        public void GetRepositoryDescriptionText_ValidRefreshState(RefreshState refreshState, bool isRepositoryListEmpty, string expectedResult)
        {
            //Arrange
            string actualResult;

            //Act
            actualResult = EmptyDataViewService.GetRepositoryDescriptionText(refreshState, isRepositoryListEmpty);

            //Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestCase(int.MinValue, true)]
        [TestCase(int.MinValue, false)]
        [TestCase(-1, true)]
        [TestCase(-1, false)]
        [TestCase(5, true)]
        [TestCase(5, false)]
        [TestCase(int.MaxValue, true)]
        [TestCase(int.MaxValue, false)]
        public void GetRepositoryTitleDescriptionTest_InvalidRefreshState(RefreshState refreshState, bool isRepositoryListEmpty)
        {
            //Arrange

            //Act

            //Assert
            Assert.Throws<NotSupportedException>(() => EmptyDataViewService.GetRepositoryDescriptionText(refreshState, isRepositoryListEmpty));
        }
    }
}