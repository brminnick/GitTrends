using System.Text.Json;
using System.Text.Json.Nodes;
using AsyncAwaitBestPractices;
using GitTrends.Common;
using Shiny.Jobs;

namespace GitTrends;

public class RetryRepositoryStarsJob(
	IAppInfo appInfo,
	IAnalyticsService analyticsService,
	RepositoryDatabase repositoryDatabase,
	GitHubApiRepositoriesService gitHubApiRepositoriesService) : IJob
{
	public const string RepositoryKey = nameof(RepositoryKey);

	static readonly WeakEventManager<Repository> _repositoryEventManager = new();
	static readonly AsyncAwaitBestPractices.WeakEventManager _jobCompletedEventManager = new();

	readonly IAnalyticsService _analyticsService = analyticsService;
	readonly RepositoryDatabase _repositoryDatabase = repositoryDatabase;
	readonly GitHubApiRepositoriesService _gitHubApiRepositoriesService = gitHubApiRepositoriesService;

	public static event EventHandler<Repository> UpdatedRepositorySavedToDatabase
	{
		add => _repositoryEventManager.AddEventHandler(value);
		remove => _repositoryEventManager.RemoveEventHandler(value);
	}

	public static event EventHandler JobCompleted
	{
		add => _jobCompletedEventManager.AddEventHandler(value);
		remove => _jobCompletedEventManager.RemoveEventHandler(value);
	}

	public string Identifier { get; } = $"{appInfo.PackageName}.{nameof(RetryRepositoryStarsJob)}";

	public string GetJobIdentifier(Repository repository) => $"{Identifier}.{repository.Url}";

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
			typeof(RetryRepositoryStarsJob),
			shouldRunInForeground,
			RequiredInternetAccess: InternetAccess.Unmetered,
			Parameters: parameterDictionary);
	}

	public async Task Run(JobInfo jobInfo, CancellationToken cancellationToken)
	{
		_analyticsService.Track($"{nameof(RetryRepositoryStarsJob)} Triggered");

		var serializedRepository = jobInfo.Parameters?[RepositoryKey] ?? throw new ArgumentNullException(nameof(jobInfo), $@"{nameof(jobInfo.Parameters)} cannot be null");
		if (Repository.TryParse(serializedRepository, out var repository) is not true)
		{
			return;
		}

		try
		{
			await foreach (var repositoryWithViewsClonesStarsData in _gitHubApiRepositoriesService.UpdateRepositoriesWithStarsData([repository], cancellationToken).ConfigureAwait(false))
			{
				await _repositoryDatabase.SaveRepository(repositoryWithViewsClonesStarsData, cancellationToken).ConfigureAwait(false);
				OnRepositorySavedToDatabase(repositoryWithViewsClonesStarsData);
			}
		}
		catch (Exception e)
		{
			_analyticsService.Report(e);
		}

		OnJobCompleted();
	}

	void OnRepositorySavedToDatabase(in Repository repository) => _repositoryEventManager.RaiseEvent(this, repository, nameof(UpdatedRepositorySavedToDatabase));
	void OnJobCompleted() => _jobCompletedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(JobCompleted));
}