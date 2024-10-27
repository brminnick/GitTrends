using GitTrends.Mobile.Common;

namespace GitTrends.UnitTests;

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
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}

	[TestCase(int.MinValue)]
	[TestCase(-1)]
	[TestCase(6)]
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
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}

	[TestCase(int.MinValue)]
	[TestCase(-1)]
	[TestCase(6)]
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
	[TestCase(RefreshState.AbuseLimit, true, "Your repositories list is empty")]
	[TestCase(RefreshState.AbuseLimit, false, "No Matching Repository Found")]
	public void GetRepositoresTitleTextTest_ValidRefreshState(RefreshState refreshState, bool isRepositoryListEmpty, string expectedResult)
	{
		//Arrange
		string actualResult;

		//Act
		actualResult = EmptyDataViewService.GetRepositoryTitleText(refreshState, isRepositoryListEmpty);

		//Assert
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}

	[TestCase(int.MinValue, true)]
	[TestCase(int.MinValue, false)]
	[TestCase(-1, true)]
	[TestCase(-1, false)]
	[TestCase(6, true)]
	[TestCase(6, false)]
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
	[TestCase(RefreshState.AbuseLimit, true, "")]
	[TestCase(RefreshState.AbuseLimit, false, "Clear search bar and try again")]
	public void GetRepositoryDescriptionText_ValidRefreshState(RefreshState refreshState, bool isRepositoryListEmpty, string expectedResult)
	{
		//Arrange
		string actualResult;

		//Act
		actualResult = EmptyDataViewService.GetRepositoryDescriptionText(refreshState, isRepositoryListEmpty);

		//Assert
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}

	[TestCase(int.MinValue, true)]
	[TestCase(int.MinValue, false)]
	[TestCase(-1, true)]
	[TestCase(-1, false)]
	[TestCase(6, true)]
	[TestCase(6, false)]
	[TestCase(int.MaxValue, true)]
	[TestCase(int.MaxValue, false)]
	public void GetRepositoryTitleDescriptionTest_InvalidRefreshState(RefreshState refreshState, bool isRepositoryListEmpty)
	{
		//Arrange

		//Act

		//Assert
		Assert.Throws<NotSupportedException>(() => EmptyDataViewService.GetRepositoryDescriptionText(refreshState, isRepositoryListEmpty));
	}

	[TestCase(RefreshState.Uninitialized, "Data not gathered")]
	[TestCase(RefreshState.Succeeded, "No traffic yet")]
	[TestCase(RefreshState.LoginExpired, "GitHub Login Expired")]
	[TestCase(RefreshState.Error, "Unable to retrieve data")]
	[TestCase(RefreshState.MaximumApiLimit, "Unable to retrieve data")]
	public void GetViewsClonesTitleText_ValidRefreshState(RefreshState refreshState, string expectedResult)
	{
		//Arrange
		string actualResult;

		//Act
		actualResult = EmptyDataViewService.GetViewsClonesTitleText(refreshState);

		//Assert
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}

	[TestCase(int.MinValue)]
	[TestCase(-1)]
	[TestCase(RefreshState.AbuseLimit)]
	[TestCase(6)]
	[TestCase(int.MaxValue)]
	public void GetViewsClonesTitleText_InvalidRefreshState(RefreshState refreshState)
	{
		//Arrange

		//Act

		//Assert
		Assert.Throws<NotSupportedException>(() => EmptyDataViewService.GetViewsClonesTitleText(refreshState));
	}

	[TestCase(RefreshState.Uninitialized, 0, "Data not gathered")]
	[TestCase(RefreshState.Uninitialized, 1, "Data not gathered")]
	[TestCase(RefreshState.Uninitialized, 2, "Data not gathered")]
	[TestCase(RefreshState.Succeeded, 0, "No Stars Yet")]
	[TestCase(RefreshState.Succeeded, 1, "Congratulations!")]
	[TestCase(RefreshState.Succeeded, 2, "No Stars Yet")]
	[TestCase(RefreshState.LoginExpired, 0, "Please login again")]
	[TestCase(RefreshState.LoginExpired, 1, "Please login again")]
	[TestCase(RefreshState.LoginExpired, 2, "Please login again")]
	[TestCase(RefreshState.Error, 0, "Unable to retrieve data")]
	[TestCase(RefreshState.Error, 1, "Unable to retrieve data")]
	[TestCase(RefreshState.Error, 2, "Unable to retrieve data")]
	[TestCase(RefreshState.MaximumApiLimit, 0, "Unable to retrieve data")]
	[TestCase(RefreshState.MaximumApiLimit, 1, "Unable to retrieve data")]
	[TestCase(RefreshState.MaximumApiLimit, 2, "Unable to retrieve data")]
	public void GetStarsTitleText_ValidRefreshState(RefreshState refreshState, double totalStars, string expectedResult)
	{
		//Arrange
		string actualResult;

		//Act
		actualResult = EmptyDataViewService.GetStarsEmptyDataViewTitleText(refreshState, totalStars);

		//Assert
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}

	[TestCase(int.MinValue, 0)]
	[TestCase(int.MinValue, 1)]
	[TestCase(int.MinValue, 2)]
	[TestCase(-1, 0)]
	[TestCase(-1, 1)]
	[TestCase(-1, 2)]
	[TestCase(RefreshState.AbuseLimit, 0)]
	[TestCase(RefreshState.AbuseLimit, 1)]
	[TestCase(RefreshState.AbuseLimit, 2)]
	[TestCase(6, 0)]
	[TestCase(6, 1)]
	[TestCase(6, 2)]
	[TestCase(int.MaxValue, 0)]
	[TestCase(int.MaxValue, 1)]
	[TestCase(int.MaxValue, 2)]
	public void GetStarsTitleText_InvalidRefreshState(RefreshState refreshState, double totalStars)
	{
		//Arrange

		//Act

		//Assert
		Assert.Throws<NotSupportedException>(() => EmptyDataViewService.GetStarsEmptyDataViewTitleText(refreshState, totalStars));
	}

	[TestCase(RefreshState.Uninitialized, "EmptyTrafficChart")]
	[TestCase(RefreshState.Succeeded, "EmptyTrafficChart")]
	[TestCase(RefreshState.LoginExpired, "EmptyTrafficChart")]
	[TestCase(RefreshState.Error, "EmptyTrafficChart")]
	[TestCase(RefreshState.MaximumApiLimit, "EmptyTrafficChart")]
	public void GetViewsClonesImage_ValidRefreshState(RefreshState refreshState, string expectedResult)
	{
		//Arrange
		string actualResult;

		//Act
		actualResult = EmptyDataViewService.GetViewsClonesImage(refreshState);

		//Assert
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}

	[TestCase(int.MinValue)]
	[TestCase(-1)]
	[TestCase(RefreshState.AbuseLimit)]
	[TestCase(6)]
	[TestCase(int.MaxValue)]
	public void GetViewsClonesImage_InvalidRefreshState(RefreshState refreshState)
	{
		//Arrange

		//Act

		//Assert
		Assert.Throws<NotSupportedException>(() => EmptyDataViewService.GetViewsClonesImage(refreshState));
	}

	[TestCase(RefreshState.Uninitialized, 0, "EmptyStarChart")]
	[TestCase(RefreshState.Uninitialized, 1, "EmptyStarChart")]
	[TestCase(RefreshState.Uninitialized, 2, "EmptyStarChart")]
	[TestCase(RefreshState.Succeeded, 0, "EmptyStarChart")]
	[TestCase(RefreshState.Succeeded, 1, "EmptyOneStarChart")]
	[TestCase(RefreshState.Succeeded, 2, "EmptyStarChart")]
	[TestCase(RefreshState.LoginExpired, 0, "EmptyStarChart")]
	[TestCase(RefreshState.LoginExpired, 1, "EmptyStarChart")]
	[TestCase(RefreshState.LoginExpired, 2, "EmptyStarChart")]
	[TestCase(RefreshState.Error, 0, "EmptyStarChart")]
	[TestCase(RefreshState.Error, 1, "EmptyStarChart")]
	[TestCase(RefreshState.Error, 2, "EmptyStarChart")]
	[TestCase(RefreshState.MaximumApiLimit, 0, "EmptyStarChart")]
	[TestCase(RefreshState.MaximumApiLimit, 1, "EmptyStarChart")]
	[TestCase(RefreshState.MaximumApiLimit, 2, "EmptyStarChart")]
	public void GetStarsImage_ValidRefreshState(RefreshState refreshState, double totalStars, string expectedResult)
	{
		//Arrange
		string actualResult;

		//Act
		actualResult = EmptyDataViewService.GetStarsEmptyDataViewImage(refreshState, totalStars);

		//Assert
		Assert.That(actualResult, Is.EqualTo(expectedResult));
	}

	[TestCase(int.MinValue, 0)]
	[TestCase(int.MinValue, 1)]
	[TestCase(int.MinValue, 2)]
	[TestCase(-1, 0)]
	[TestCase(-1, 1)]
	[TestCase(-1, 2)]
	[TestCase(RefreshState.AbuseLimit, 0)]
	[TestCase(RefreshState.AbuseLimit, 1)]
	[TestCase(RefreshState.AbuseLimit, 2)]
	[TestCase(6, 0)]
	[TestCase(6, 1)]
	[TestCase(6, 2)]
	[TestCase(int.MaxValue, 0)]
	[TestCase(int.MaxValue, 1)]
	[TestCase(int.MaxValue, 2)]
	public void GetStarsImage_InvalidRefreshState(RefreshState refreshState, double totalStars)
	{
		//Arrange

		//Act

		//Assert
		Assert.Throws<NotSupportedException>(() => EmptyDataViewService.GetStarsEmptyDataViewImage(refreshState, totalStars));
	}
}