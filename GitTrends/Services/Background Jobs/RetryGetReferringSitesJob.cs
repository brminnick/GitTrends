using System.Text.Json;
using AsyncAwaitBestPractices;
using GitTrends.Common;
using Shiny.Jobs;
namespace GitTrends;

public class RetryGetReferringSitesJob(
	IAppInfo appInfo,
	IAnalyticsService analyticsService,
	ReferringSitesDatabase referringSitesDatabase,
	GitHubApiRepositoriesService gitHubApiRepositoriesService) : IJob
{
	public const string RepositoryKey = nameof(RepositoryKey);

	static readonly WeakEventManager<Repository> _jobCompletedEventManager = new();
	static readonly WeakEventManager<MobileReferringSiteModel> _mobileReferringSiteRetrievedEventManager = new();

	readonly IAnalyticsService _analyticsService = analyticsService;
	readonly ReferringSitesDatabase _referringSitesDatabase = referringSitesDatabase;
	readonly GitHubApiRepositoriesService _gitHubApiRepositoriesService = gitHubApiRepositoriesService;

	public string Identifier { get; } = $"{appInfo.PackageName}.{nameof(RetryGetReferringSitesJob)}";

	public JobInfo GetJobInfo(Repository repository, bool shouldRunInForeground)
	{
		var serializedRepository = JsonSerializer.Serialize(repository);

		var parameterDictionary = new Dictionary<string, string>
		{
			{
				RepositoryKey, serializedRepository
			}
		};

		return new(
			GetJobIdentifier(repository),
			typeof(RetryRepositoriesViewsClonesStarsJob),
			shouldRunInForeground,
			RequiredInternetAccess: InternetAccess.Unmetered,
			Parameters: parameterDictionary);
	}

	public string GetJobIdentifier(Repository repository) => $"{Identifier}.{repository.Url}";

	public static event EventHandler<Repository> JobCompleted
	{
		add => _jobCompletedEventManager.AddEventHandler(value);
		remove => _jobCompletedEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler<MobileReferringSiteModel> MobileReferringSiteRetrieved
	{
		add => _mobileReferringSiteRetrievedEventManager.AddEventHandler(value);
		remove => _mobileReferringSiteRetrievedEventManager.RemoveEventHandler(value);
	}

	public async Task Run(JobInfo jobInfo, CancellationToken cancellationToken)
	{
		_analyticsService.Track($"{nameof(BackgroundFetchService)}.{nameof(RetryGetReferringSitesJob)} Triggered");

		var serializedRepository = jobInfo.Parameters?[RepositoryKey] ?? throw new ArgumentNullException(nameof(jobInfo), $@"{nameof(jobInfo.Parameters)} cannot be null");

		if (Repository.TryParse(serializedRepository, out var repository) is not true)
		{
			return;
		}

		try
		{
			var referringSites = await _gitHubApiRepositoriesService.GetReferringSites(repository, cancellationToken).ConfigureAwait(false);

			await foreach (var mobileReferringSite in _gitHubApiRepositoriesService.GetMobileReferringSites(referringSites, repository.Url, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token).ConfigureAwait(false))
			{
				await _referringSitesDatabase.SaveReferringSite(mobileReferringSite, repository.Url, cancellationToken).ConfigureAwait(false);
				OnMobileReferringSiteRetrieved(mobileReferringSite);
			}
		}
		catch (Exception e)
		{
			_analyticsService.Report(e);
		}
		finally
		{
			OnJobCompleted(repository);
		}
	}

	void OnJobCompleted(in Repository repository) =>
		_jobCompletedEventManager.RaiseEvent(this, repository, nameof(JobCompleted));

	void OnMobileReferringSiteRetrieved(in MobileReferringSiteModel referringSite) =>
		_mobileReferringSiteRetrievedEventManager.RaiseEvent(this, referringSite, nameof(MobileReferringSiteRetrieved));
}