using Shiny;
using Shiny.Jobs;

namespace GitTrends.UnitTests;

class MockJobManager : IJobManager
{
	readonly Dictionary<string, JobInfo> _jobDictionary = [];

	public bool IsRunning { get; private set; }

	public IObservable<JobInfo> JobStarted => throw new NotImplementedException();

	public IObservable<JobRunResult> JobFinished => throw new NotImplementedException();

	public void Cancel(string jobIdentifier)
	{
		_jobDictionary.Remove(jobIdentifier, out _);
	}

	public void CancelAll()
	{
		_jobDictionary.Clear();
	}

	public JobInfo? GetJob(string jobIdentifier)
	{
		return _jobDictionary.GetValueOrDefault(jobIdentifier);
	}

	public IList<JobInfo> GetJobs() => [.. _jobDictionary.Values];


	public void Register(JobInfo jobInfo)
	{
		_jobDictionary.Add(jobInfo.Identifier, jobInfo);
	}

	public Task<AccessState> RequestAccess() => Task.FromResult(AccessState.Available);

	public async Task<JobRunResult> Run(string jobIdentifier, CancellationToken cancelToken = default)
	{
		var jobInfo = _jobDictionary[jobIdentifier];

		var job = (IJob)(Activator.CreateInstance(jobInfo.JobType) ?? throw new InvalidOperationException());

		try
		{
			await job.Run(jobInfo, cancelToken).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			return new JobRunResult(jobInfo, e);
		}

		return new JobRunResult(jobInfo, null);
	}

	public async Task<IEnumerable<JobRunResult>> RunAll(CancellationToken cancelToken = default, bool runSequentially = false)
	{
		if (!runSequentially)
			return await Task.WhenAll(_jobDictionary.Select(x => Run(x.Key, cancelToken))).ConfigureAwait(false);

		var resultList = new List<JobRunResult>();

		foreach (var job in _jobDictionary)
		{
			var result = await Run(job.Key, CancellationToken.None).ConfigureAwait(false);
			resultList.Add(result);
		}

		return resultList;

	}

	public async void RunTask(string taskName, Func<CancellationToken, Task> task)
	{
		IsRunning = true;

		try
		{
			await task(CancellationToken.None).ConfigureAwait(false);
		}
		finally
		{
			IsRunning = false;
		}
	}
}