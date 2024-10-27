using System.Text.Json;
using AsyncAwaitBestPractices;
using GitTrends.Common;
using Shiny.Jobs;
namespace GitTrends;

public class RetryRepositoriesViewsClonesStarsJob(
	IAppInfo appInfo,
	IAnalyticsService analyticsService,
	RepositoryDatabase repositoryDatabase,
	GitHubApiRepositoriesService gitHubApiRepositoriesService)
	: IJob
{
	static readonly WeakEventManager<Repository> _jobCompletedEventManager = new();

	public const string RepositoryKey = nameof(RepositoryKey);

	readonly IAnalyticsService _analyticsService = analyticsService;
	readonly RepositoryDatabase _repositoryDatabase = repositoryDatabase;
	readonly GitHubApiRepositoriesService _gitHubApiRepositoriesService = gitHubApiRepositoriesService;

	public static event EventHandler<Repository> JobCompleted
	{
		add => _jobCompletedEventManager.AddEventHandler(value);
		remove => _jobCompletedEventManager.RemoveEventHandler(value);
	}

	public string Identifier { get; } = $"{appInfo.PackageName}.{nameof(RetryRepositoriesViewsClonesStarsJob)}";

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

	public async Task Run(JobInfo jobInfo, CancellationToken cancellationToken)
	{
		_analyticsService.Track($"{nameof(RetryOrganizationsRepositoriesJob)} Triggered");

		var serializedRepository = jobInfo.Parameters?[RepositoryKey] ?? throw new ArgumentNullException(nameof(jobInfo), $@"{nameof(jobInfo.Parameters)} cannot be null");
		if (Repository.TryParse(serializedRepository, out var repository) is not true)
		{
			return;
		}

		try
		{
			await foreach (var repositoryWithViewsClonesData in _gitHubApiRepositoriesService.UpdateRepositoriesWithViewsAndClonesData(
			[
				repository
			], cancellationToken).ConfigureAwait(false))
			{
				repository = repositoryWithViewsClonesData;
			}

			await foreach (var repositoryWithViewsClonesStarsData in _gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData(
			[
				repository
			], cancellationToken).ConfigureAwait(false))
			{
				await _repositoryDatabase.SaveRepository(repositoryWithViewsClonesStarsData, cancellationToken).ConfigureAwait(false);
				OnJobCompleted(repositoryWithViewsClonesStarsData);
			}

		}
		catch (Exception e)
		{
			_analyticsService.Report(e);
		}
	}

	void OnJobCompleted(in Repository repository)
	{
		_jobCompletedEventManager.RaiseEvent(this, repository, nameof(JobCompleted));
	}
}