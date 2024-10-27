using GitTrends.Common;
using Shiny.Jobs;

namespace GitTrends;

public class CleanDatabaseJob(
	IAppInfo appInfo,
	IAnalyticsService analyticsService,
	RepositoryDatabase repositoryDatabase,
	ReferringSitesDatabase referringSitesDatabase) : IJob
{
	static readonly AsyncAwaitBestPractices.WeakEventManager _jobCompletedEventManager = new();

	readonly IAnalyticsService _analyticsService = analyticsService;
	readonly RepositoryDatabase _repositoryDatabase = repositoryDatabase;
	readonly ReferringSitesDatabase _referringSitesDatabase = referringSitesDatabase;

	public string Identifier { get; } = $"{appInfo.PackageName}.{nameof(CleanDatabaseJob)}";

	public static event EventHandler JobCompleted
	{
		add => _jobCompletedEventManager.AddEventHandler(value);
		remove => _jobCompletedEventManager.RemoveEventHandler(value);
	}

	public JobInfo GetJobInfo(bool shouldRunInForeground) => new(
		Identifier,
		typeof(RetryRepositoriesViewsClonesStarsJob),
		shouldRunInForeground,
		RequiredInternetAccess: InternetAccess.None);


	public async Task Run(JobInfo jobInfo, CancellationToken cancellationToken)
	{
		_analyticsService.Track($"{nameof(CleanDatabaseJob)} Triggered");

		try
		{
			await Task.WhenAll(_referringSitesDatabase.DeleteExpiredData(cancellationToken), _repositoryDatabase.DeleteExpiredData(cancellationToken)).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			_analyticsService.Report(e);
		}
		finally
		{
			OnCleanDatabaseJobCompleted();
		}
	}

	void OnCleanDatabaseJobCompleted()
	{
		_jobCompletedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(JobCompleted));
	}
}