using GitTrends.Common;

namespace GitTrends.UnitTests;

class GitTrendsStatisticsServiceTests : BaseTest
{
	[Test]
	public async Task InitializeTest()
	{
		//Arrange
		string? clientId_Initial, clientId_Final;
		IReadOnlyList<Contributor> contributors_Initial, contributors_Final;
		long? watchers_Initial, watchers_Final, stars_Initial, stars_Final, forks_Initial, forks_Final;
		Uri? enableOrganizationsUri_Initial, enableOrganizationsUri_Final, gitHubUri_Initial, gitHubUri_Final;

		var gitTrendsStatisticsService = ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsStatisticsService>();

		//Act
		stars_Initial = gitTrendsStatisticsService.Stars;
		forks_Initial = gitTrendsStatisticsService.Forks;
		watchers_Initial = gitTrendsStatisticsService.Watchers;
		clientId_Initial = gitTrendsStatisticsService.ClientId;
		gitHubUri_Initial = gitTrendsStatisticsService.GitHubUri;
		contributors_Initial = gitTrendsStatisticsService.Contributors;
		enableOrganizationsUri_Initial = gitTrendsStatisticsService.EnableOrganizationsUri;

		await gitTrendsStatisticsService.Initialize(CancellationToken.None).ConfigureAwait(false);

		stars_Final = gitTrendsStatisticsService.Stars;
		forks_Final = gitTrendsStatisticsService.Forks;
		watchers_Final = gitTrendsStatisticsService.Watchers;
		clientId_Final = gitTrendsStatisticsService.ClientId;
		gitHubUri_Final = gitTrendsStatisticsService.GitHubUri;
		contributors_Final = gitTrendsStatisticsService.Contributors;
		enableOrganizationsUri_Final = gitTrendsStatisticsService.EnableOrganizationsUri;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(stars_Initial, Is.Null);
			Assert.That(forks_Initial, Is.Null);
			Assert.That(watchers_Initial, Is.Null);
			Assert.That(clientId_Initial, Is.Null);
			Assert.That(gitHubUri_Initial, Is.Null);
			Assert.That(enableOrganizationsUri_Initial, Is.Null);

			Assert.That(contributors_Initial, Is.Not.Null);
			Assert.That(contributors_Initial, Is.Empty);

			Assert.That(stars_Final, Is.Not.Null);
			Assert.That(forks_Final, Is.Not.Null);
			Assert.That(watchers_Final, Is.Not.Null);
			Assert.That(clientId_Final, Is.Not.Null);
			Assert.That(gitHubUri_Final, Is.Not.Null);
			Assert.That(enableOrganizationsUri_Final, Is.Not.Null);

			Assert.That(stars_Final, Is.GreaterThan(0));
			Assert.That(forks_Final, Is.GreaterThan(0));
			Assert.That(watchers_Final, Is.GreaterThan(0));

			Assert.That(contributors_Final, Is.Not.Empty);
		});

		foreach (var contributor in contributors_Final)
		{
			var isAvatarUrlValid = Uri.TryCreate(contributor.AvatarUrl.ToString(), UriKind.Absolute, out _);
			var isGitHubUrlValid = Uri.TryCreate(contributor.GitHubUrl.ToString(), UriKind.Absolute, out _);

			Assert.Multiple(() =>
			{
				Assert.That(isAvatarUrlValid);
				Assert.That(isGitHubUrlValid);
				Assert.That(contributor.ContributionCount, Is.GreaterThan(0));
				Assert.That(string.IsNullOrEmpty(contributor.Login), Is.False);
			});
		}
	}
}