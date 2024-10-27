using System.Globalization;
using System.Net;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Refit;

[assembly: NonParallelizable]
namespace GitTrends.UnitTests;

abstract class BaseTest : IDisposable
{
	protected static string AuthenticatedGitHubUserAvatarUrl { get; } = "https://avatars.githubusercontent.com/u/13558917?u=6e0d77ca0420f418c8ad5110cb155dea5d427a35&v=4";
	protected CancellationTokenSource TestCancellationTokenSource { get; private set; } = new();

	protected static DecompressionMethods GetDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli;

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	[TearDown]
	public virtual Task TearDown()
	{
		var extendedBackgroundFetchService = (ExtendedBackgroundFetchService)ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		extendedBackgroundFetchService.CancelAllJobs();

		return Task.CompletedTask;
	}

	[SetUp]
	public virtual async Task Setup()
	{
		TestCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

		CultureInfo.DefaultThreadCurrentCulture = null;
		CultureInfo.DefaultThreadCurrentUICulture = null;

		InitializeServiceCollection();

		var extendedBackgroundFetchService = (ExtendedBackgroundFetchService)ServiceCollection.ServiceProvider.GetRequiredService<BackgroundFetchService>();
		extendedBackgroundFetchService.CancelAllJobs();

		var preferences = ServiceCollection.ServiceProvider.GetRequiredService<IPreferences>();
		preferences.Clear();

		var secureStorage = ServiceCollection.ServiceProvider.GetRequiredService<ISecureStorage>();
		secureStorage.RemoveAll();

		var referringSitesDatabase = ServiceCollection.ServiceProvider.GetRequiredService<ReferringSitesDatabase>();
		await referringSitesDatabase.DeleteAllData(TestCancellationTokenSource.Token).ConfigureAwait(false);

		var repositoryDatabase = ServiceCollection.ServiceProvider.GetRequiredService<RepositoryDatabase>();
		await repositoryDatabase.DeleteAllData(TestCancellationTokenSource.Token).ConfigureAwait(false);

		var gitHubAuthenticationService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubAuthenticationService>();
		await gitHubAuthenticationService.LogOut(TestCancellationTokenSource.Token).ConfigureAwait(false);

		var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();
		await notificationService.SetAppBadgeCount(0, TestCancellationTokenSource.Token).ConfigureAwait(false);
		notificationService.UnRegister();
	}

	protected static async Task AuthenticateUser(GitHubUserService gitHubUserService, GitHubGraphQLApiService gitHubGraphQLApiService, CancellationToken token)
	{
		var accessToken = await Mobile.Common.AzureFunctionsApiService.GetTestToken(token).ConfigureAwait(false);
		if (accessToken.IsEmpty() || string.IsNullOrWhiteSpace(accessToken.AccessToken))
			throw new InvalidOperationException("Invalid Token");

		await gitHubUserService.SaveGitHubToken(accessToken).ConfigureAwait(false);

		var (login, name, avatarUri) = await gitHubGraphQLApiService.GetCurrentUserInfo(token).ConfigureAwait(false);

		gitHubUserService.Alias = login;
		gitHubUserService.Name = name;
		gitHubUserService.AvatarUrl = avatarUri.ToString();
	}

	protected static Repository CreateRepository(bool createViewsAndClones = true)
	{
		const string gitTrendsAvatarUrl = "https://avatars3.githubusercontent.com/u/61480020?s=400&u=b1a900b5fa1ede22af9d2d9bfd6c49a072e659ba&v=4";
		var downloadedAt = DateTimeOffset.UtcNow;

		var dailyViewsList = new List<DailyViewsModel>();
		var dailyClonesList = new List<DailyClonesModel>();

		for (int i = 0; i < 14 && createViewsAndClones; i++)
		{
			var count = DemoDataConstants.GetRandomNumber();
			var uniqueCount = count / 2; //Ensures uniqueCount is always less than count

			dailyViewsList.Add(new DailyViewsModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqueCount));
			dailyClonesList.Add(new DailyClonesModel(downloadedAt.Subtract(TimeSpan.FromDays(i)), count, uniqueCount));
		}

		IList<DateTimeOffset> starredAt = [.. DemoDataConstants.GenerateStarredAtDates(DemoDataConstants.GetRandomNumber(1))];

		return new Repository($"Repository " + DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomText(), DemoDataConstants.GetRandomNumber(),
			DemoUserConstants.Alias, gitTrendsAvatarUrl,
			DemoDataConstants.GetRandomNumber(), DemoDataConstants.GetRandomNumber(), starredAt.Count,
			gitTrendsAvatarUrl, false, downloadedAt, RepositoryPermission.ADMIN, false, false, dailyViewsList, dailyClonesList, starredAt);
	}

	protected static MobileReferringSiteModel CreateMobileReferringSite(DateTimeOffset downloadedAt, string referrer)
	{
		return new MobileReferringSiteModel(new ReferringSiteModel(DemoDataConstants.GetRandomNumber(),
			DemoDataConstants.GetRandomNumber(),
			referrer))
		{
			DownloadedAt = downloadedAt
		};
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			TestCancellationTokenSource.Dispose();
		}
	}

	protected virtual void InitializeServiceCollection()
	{
		var handler = new HttpClientHandler
		{
			AutomaticDecompression = GetDecompressionMethods()
		};

		var gitHubApiV3Client = RestService.For<IGitHubApiV3>(new HttpClient(handler)
		{
			BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl)
		});

		var gitHubGraphQLCLient = RestService.For<IGitHubGraphQLApi>(new HttpClient(handler)
		{
			BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi)
		});

		var azureFunctionsClient = RestService.For<IAzureFunctionsApi>(new HttpClient(handler)
		{
			BaseAddress = new Uri(AzureConstants.AzureFunctionsApiUrl)
		});

		ServiceCollection.Initialize(azureFunctionsClient, gitHubApiV3Client, gitHubGraphQLCLient);
	}
}