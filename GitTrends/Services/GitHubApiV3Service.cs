using GitTrends.Common;
using GitTrends.Mobile.Common;

namespace GitTrends;

public class GitHubApiV3Service(
	IGitHubApiV3 gitHubApiV3,
	IAnalyticsService analyticsService,
	GitHubUserService gitHubUserService) : BaseMobileApiService(analyticsService)
{
	readonly IGitHubApiV3 _githubApiClient = gitHubApiV3;
	readonly GitHubUserService _gitHubUserService = gitHubUserService;

	public Task<RepositoryFile> GetGitTrendsFile(string fileName, CancellationToken cancellationToken) => _githubApiClient.GetGitTrendsFile(fileName, cancellationToken);

	public async Task<RepositoryViewsModel> GetRepositoryViewStatistics(string owner, string repo, CancellationToken cancellationToken)
	{
		if (_gitHubUserService.IsDemoUser)
		{
			//Yield off of the main thread to generate dailyViewsModelList
			await Task.Yield();

			var dailyViewsModelList = new List<DailyViewsModel>();

			for (int i = 0; i < 14; i++)
			{
				var count = DemoDataConstants.GetRandomNumber();
				var uniqueCount = count / 2; //Ensures uniqueCount is always less than count

				//Ensures one Demo repo is Trending
				if (i is 13 && new Random().Next(0, DemoDataConstants.RepoCount) is DemoDataConstants.RepoCount - 1 or DemoDataConstants.RepoCount - 2)
					dailyViewsModelList.Add(new DailyViewsModel(DateTimeOffset.UtcNow, DemoDataConstants.MaximumRandomNumber * 4, DemoDataConstants.MaximumRandomNumber / 2));
				else
					dailyViewsModelList.Add(new DailyViewsModel(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(i)), count, uniqueCount));
			}

			return new RepositoryViewsModel(dailyViewsModelList.Sum(static x => x.TotalViews), dailyViewsModelList.Sum(static x => x.TotalUniqueViews), dailyViewsModelList, repo, owner);
		}

		var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
		var response = await _githubApiClient.GetRepositoryViewStatistics(owner, repo, GetGitHubBearerTokenHeader(token), cancellationToken).ConfigureAwait(false);

		return new RepositoryViewsModel(response.TotalCount, response.TotalUniqueCount, response.DailyViewsList, repo, owner);
	}

	public async Task<RepositoryClonesModel> GetRepositoryCloneStatistics(string owner, string repo, CancellationToken cancellationToken)
	{
		if (_gitHubUserService.IsDemoUser)
		{
			//Yield off of the main thread to generate dailyViewsModelList
			await Task.Yield();

			var dailyClonesModelList = new List<DailyClonesModel>();

			for (int i = 0; i < 14; i++)
			{
				var count = DemoDataConstants.GetRandomNumber() / 2; //Ensures the average clone count is smaller than the average view count
				var uniqueCount = count / 2; //Ensures uniqueCount is always less than count

				dailyClonesModelList.Add(new DailyClonesModel(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(i)), count, uniqueCount));
			}

			return new RepositoryClonesModel(dailyClonesModelList.Sum(static x => x.TotalClones), dailyClonesModelList.Sum(static x => x.TotalUniqueClones), dailyClonesModelList, repo, owner);
		}

		var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
		var response = await _githubApiClient.GetRepositoryCloneStatistics(owner, repo, GetGitHubBearerTokenHeader(token), cancellationToken).ConfigureAwait(false);

		return new RepositoryClonesModel(response.TotalCount, response.TotalUniqueCount, response.DailyClonesList, repo, owner);
	}

	public async Task<HttpResponseMessage> GetGitHubApiResponse(CancellationToken cancellationToken)
	{
		var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);

		if (!_gitHubUserService.IsAuthenticated)
			return await _githubApiClient.GetGitHubApiResponse_Unauthenticated(cancellationToken, GetGitHubBearerTokenHeader(token)).ConfigureAwait(false);

		return await _githubApiClient.GetGitHubApiResponse_Authenticated(GetGitHubBearerTokenHeader(token), cancellationToken).ConfigureAwait(false);
	}

	public async Task<IReadOnlyList<ReferringSiteModel>> GetReferringSites(string owner, string repo, CancellationToken cancellationToken)
	{
		if (_gitHubUserService.IsDemoUser)
		{
			//Yield off of main thread to generate MobileReferringSiteModels
			await Task.Yield();

			var referringSitesList = new List<ReferringSiteModel>();

			for (int i = 0; i < DemoDataConstants.ReferringSitesCount; i++)
			{
				string referrer;
				do
				{
					referrer = DemoDataConstants.GetRandomText();
				} while (Uri.TryCreate($"{Uri.UriSchemeHttps}://{referrer}", UriKind.Absolute, out _));

				referringSitesList.Add(new ReferringSiteModel(DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomNumber(), referrer));
			}

			return referringSitesList;
		}

		var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
		var referringSites = await _githubApiClient.GetReferringSites(owner, repo, GetGitHubBearerTokenHeader(token), cancellationToken).ConfigureAwait(false);

		return [.. referringSites.Select(static x => new ReferringSiteModel(x.Count, x.Uniques, x.Referrer))];
	}

	public async Task<StarGazers> GetStarGazers(string owner, string repo, CancellationToken cancellationToken, int starGazersPerRequest = 100)
	{
		if (_gitHubUserService.IsDemoUser)
		{
			var starCount = DemoDataConstants.GetRandomNumber();
			var starredAtDates = DemoDataConstants.GenerateStarredAtDates(starCount);

			return new StarGazers(starCount, [.. starredAtDates.Select(static x => new StarGazerInfo(x, string.Empty))]);
		}

		var totalStarGazers = new List<StarGazer>();

		IReadOnlyList<StarGazer> starGazerResponse;
		int currentPageNumber = 1;
		do
		{
			var token = await _gitHubUserService.GetGitHubToken().ConfigureAwait(false);
			starGazerResponse = await _githubApiClient.GetStarGazers(owner, repo, currentPageNumber, GetGitHubBearerTokenHeader(token), cancellationToken, starGazersPerRequest).ConfigureAwait(false);

			totalStarGazers.AddRange(starGazerResponse);
			currentPageNumber++;
		} while (starGazerResponse.Count > 0);

		return new StarGazers(totalStarGazers.Count, [.. totalStarGazers.Select(static x => new StarGazerInfo(x.StarredAt, string.Empty))]);
	}
}