using GitTrends.Common;
namespace GitTrends.UnitTests.BackgroundJobs;

class CleanDatabaseJobTests : BaseJobTest
{
	[Test]
	public async Task CleanUpDatabaseTest()
	{
		//Arrange
		bool wasScheduledSuccessfully_First, wasScheduledSuccessfully_Second;
		var databaseCleanupCompletedTCS = new TaskCompletionSource();
		CleanDatabaseJob.JobCompleted += HandleCleanDatabaseCompleted;

		int repositoryDatabaseCount_Initial, repositoryDatabaseCount_Final, referringSitesDatabaseCount_Initial, referringSitesDatabaseCount_Final;
		MobileReferringSiteModel expiredReferringSite_Initial, unexpiredReferringSite_Initial;
		Repository expiredRepository_Initial, unexpiredRepository_Initial, expiredRepository_Final, unexpiredRepository_Final;

		var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
		var backgroundFetchService = ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		var referringSitesDatabase = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesDatabase>();

		expiredRepository_Initial = CreateRepository(DateTimeOffset.UtcNow.Subtract(repositoryDatabase.ExpiresAt), "https://github.com/brminnick/gittrends");
		unexpiredRepository_Initial = CreateRepository(DateTimeOffset.UtcNow, "https://github.com/brminnick/gitstatus");

		expiredReferringSite_Initial = CreateMobileReferringSite(DateTimeOffset.UtcNow.Subtract(referringSitesDatabase.ExpiresAt), "Google");
		unexpiredReferringSite_Initial = CreateMobileReferringSite(DateTimeOffset.UtcNow, "codetraveler.io");

		await repositoryDatabase.SaveRepository(expiredRepository_Initial, TestCancellationTokenSource.Token).ConfigureAwait(false);
		await repositoryDatabase.SaveRepository(unexpiredRepository_Initial, TestCancellationTokenSource.Token).ConfigureAwait(false);

		await referringSitesDatabase.SaveReferringSite(expiredReferringSite_Initial, expiredRepository_Initial.Url, TestCancellationTokenSource.Token).ConfigureAwait(false);
		await referringSitesDatabase.SaveReferringSite(unexpiredReferringSite_Initial, unexpiredRepository_Initial.Url, TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Act
		repositoryDatabaseCount_Initial = await getRepositoryDatabaseCount(repositoryDatabase).ConfigureAwait(false);
		referringSitesDatabaseCount_Initial = await getReferringSitesDatabaseCount(referringSitesDatabase, expiredRepository_Initial.Url, unexpiredRepository_Initial.Url).ConfigureAwait(false);

		wasScheduledSuccessfully_First = backgroundFetchService.TryScheduleCleanUpDatabase();
		wasScheduledSuccessfully_Second = backgroundFetchService.TryScheduleCleanUpDatabase();

		await databaseCleanupCompletedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		repositoryDatabaseCount_Final = await getRepositoryDatabaseCount(repositoryDatabase).ConfigureAwait(false);
		referringSitesDatabaseCount_Final = await getReferringSitesDatabaseCount(referringSitesDatabase, expiredRepository_Initial.Url, unexpiredRepository_Initial.Url).ConfigureAwait(false);

		var finalRepositories = await repositoryDatabase.GetRepositories(TestCancellationTokenSource.Token).ConfigureAwait(false);
		expiredRepository_Final = finalRepositories.First(x => x.DataDownloadedAt == expiredRepository_Initial.DataDownloadedAt);
		unexpiredRepository_Final = finalRepositories.First(x => x.DataDownloadedAt == unexpiredRepository_Initial.DataDownloadedAt);

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(wasScheduledSuccessfully_First);
			Assert.That(wasScheduledSuccessfully_Second, Is.False);

			Assert.That(repositoryDatabaseCount_Initial, Is.EqualTo(2));
			Assert.That(referringSitesDatabaseCount_Initial, Is.EqualTo(2));

			Assert.That(repositoryDatabaseCount_Final, Is.EqualTo(repositoryDatabaseCount_Initial));

			Assert.That(expiredRepository_Initial.DailyClonesList?.Sum(static x => x.TotalClones), Is.EqualTo(0));
			Assert.That(expiredRepository_Initial.DailyClonesList?.Sum(static x => x.TotalUniqueClones), Is.EqualTo(0));
			Assert.That(expiredRepository_Initial.DailyViewsList?.Sum(static x => x.TotalViews), Is.EqualTo(0));
			Assert.That(expiredRepository_Initial.DailyViewsList?.Sum(static x => x.TotalUniqueViews), Is.EqualTo(0));

			Assert.That(expiredRepository_Final.DailyClonesList, Is.Null);
			Assert.That(expiredRepository_Final.DailyViewsList, Is.Null);

			Assert.That(unexpiredRepository_Final.DailyClonesList?.Sum(static x => x.TotalClones), Is.EqualTo(unexpiredRepository_Initial.DailyClonesList?.Sum(static x => x.TotalClones)));
			Assert.That(unexpiredRepository_Final.DailyClonesList?.Sum(static x => x.TotalUniqueClones), Is.EqualTo(unexpiredRepository_Initial.DailyClonesList?.Sum(static x => x.TotalUniqueClones)));
			Assert.That(unexpiredRepository_Final.DailyViewsList?.Sum(static x => x.TotalViews), Is.EqualTo(unexpiredRepository_Initial.DailyViewsList?.Sum(static x => x.TotalViews)));
			Assert.That(unexpiredRepository_Final.DailyViewsList?.Sum(static x => x.TotalUniqueViews), Is.EqualTo(unexpiredRepository_Initial.DailyViewsList?.Sum(static x => x.TotalUniqueViews)));

			Assert.That(repositoryDatabaseCount_Final, Is.EqualTo(2));
			Assert.That(referringSitesDatabaseCount_Final, Is.EqualTo(1));

			Assert.That(unexpiredRepository_Initial.IsFavorite, Is.True);
			Assert.That(unexpiredRepository_Final.IsFavorite, Is.True);
		});

		async Task<int> getRepositoryDatabaseCount(RepositoryDatabase repositoryDatabase)
		{
			var repositories = await repositoryDatabase.GetRepositories(TestCancellationTokenSource.Token).ConfigureAwait(false);
			return repositories.Count;
		}

		async Task<int> getReferringSitesDatabaseCount(ReferringSitesDatabase referringSitesDatabase, params string[] repositoryUrl)
		{
			var referringSitesCount = 0;

			foreach (var url in repositoryUrl)
			{
				var referringSites = await referringSitesDatabase.GetReferringSites(url, TestCancellationTokenSource.Token).ConfigureAwait(false);
				referringSitesCount += referringSites.Count();
			}
			return referringSitesCount;
		}

		void HandleCleanDatabaseCompleted(object? sender, EventArgs e)
		{
			CleanDatabaseJob.JobCompleted -= HandleCleanDatabaseCompleted;
			databaseCleanupCompletedTCS.SetResult();
		}
	}
}