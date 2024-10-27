using System.Text.Json;
using AsyncAwaitBestPractices;
using GitTrends.Common;
using Shiny.Jobs;

namespace GitTrends;

public class RetryOrganizationsRepositoriesJob(
	IAppInfo appInfo,
	IAnalyticsService analyticsService,
	GitHubGraphQLApiService gitHubGraphQlApiService,
	RetryRepositoriesViewsClonesStarsJob retryRepositoriesViewsClonesStarsJob)
	: IJob
{
	static readonly WeakEventManager<string> _jobCompletedEventManager = new();

	public const string OrganizationNameKey = nameof(OrganizationNameKey);

	public static event EventHandler<string> JobCompleted
	{
		add => _jobCompletedEventManager.AddEventHandler(value);
		remove => _jobCompletedEventManager.RemoveEventHandler(value);
	}

	public string Identifier { get; } = $"{appInfo.PackageName}.{nameof(RetryOrganizationsRepositoriesJob)}";

	public string GetJobIdentifier(string organizationName) => $"{Identifier}.{organizationName}";

	public JobInfo GetJobInfo(string organizationName, bool shouldRunInForeground)
	{
		var parameterDictionary = new Dictionary<string, string>
		{
			{
				OrganizationNameKey, organizationName
			}
		};

		return new JobInfo(
			GetJobIdentifier(organizationName),
			typeof(RetryOrganizationsRepositoriesJob),
			shouldRunInForeground,
			RequiredInternetAccess:
			InternetAccess.Unmetered, Parameters: parameterDictionary);
	}

	public async Task Run(JobInfo jobInfo, CancellationToken cancellationToken)
	{
		analyticsService.Track($"{nameof(RetryOrganizationsRepositoriesJob)} Triggered");

		var organizationName = jobInfo.Parameters?[OrganizationNameKey] ?? throw new ArgumentNullException(nameof(jobInfo), $@"{nameof(jobInfo.Parameters)} cannot be null");

		try
		{
			List<Task> retryRepositoriesViewsClonesStarsJobTaskList = [];

			var repositories = await gitHubGraphQlApiService.GetOrganizationRepositories(organizationName, cancellationToken).ConfigureAwait(false);

			foreach (var repository in repositories)
			{
				var retryRepositoriesViewsClonesStarsJobTask = retryRepositoriesViewsClonesStarsJob.Run(retryRepositoriesViewsClonesStarsJob.GetJobInfo(repository, jobInfo.RunOnForeground), cancellationToken);
				retryRepositoriesViewsClonesStarsJobTaskList.Add(retryRepositoriesViewsClonesStarsJobTask);
			}

			await Task.WhenAll(retryRepositoriesViewsClonesStarsJobTaskList).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			analyticsService.Report(e);
		}
		finally
		{
			OnJobCompleted(organizationName);
		}
	}

	void OnJobCompleted(in string organizationName)
	{
		_jobCompletedEventManager.RaiseEvent(this, organizationName, nameof(JobCompleted));
	}
}