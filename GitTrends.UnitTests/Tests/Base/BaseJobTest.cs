using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends.UnitTests;

abstract class BaseJobTest : BaseTest
{
	protected static Repository CreateRepository(DateTimeOffset downloadedAt, string repositoryUrl)
	{
		var starredAtList = new List<DateTimeOffset>();
		var dailyViewsList = new List<DailyViewsModel>();
		var dailyClonesList = new List<DailyClonesModel>();

		for (int i = 0; i < 14; i++)
		{
			var count = DemoDataConstants.GetRandomNumber();
			var uniqueCount = count / 2; //Ensures uniqueCount is always less than count

			starredAtList.Add(DemoDataConstants.GetRandomDate());
			dailyViewsList.Add(new DailyViewsModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqueCount));
			dailyClonesList.Add(new DailyClonesModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqueCount));
		}

		return new Repository($"Repository " + DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomNumber(),
			DemoUserConstants.Alias, GitHubConstants.GitTrendsAvatarUrl, DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomNumber(), starredAtList.Count,
			repositoryUrl, false, downloadedAt, RepositoryPermission.ADMIN, false, true, dailyViewsList, dailyClonesList, starredAtList);
	}
}