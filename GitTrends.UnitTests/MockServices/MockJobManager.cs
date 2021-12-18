using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shiny;
using Shiny.Jobs;

namespace GitTrends.UnitTests;

class MockJobManager : IJobManager
{
	readonly Dictionary<string, JobInfo> _jobDictionary = new();

	public bool IsRunning { get; private set; }

	public IObservable<JobInfo> JobStarted => throw new NotImplementedException();

	public IObservable<JobRunResult> JobFinished => throw new NotImplementedException();

	public Task Cancel(string jobIdentifier)
	{
		if (_jobDictionary.TryGetValue(jobIdentifier, out _))
			_jobDictionary.Remove(jobIdentifier);

		return Task.CompletedTask;
	}

	public Task CancelAll()
	{
		_jobDictionary.Clear();
		return Task.CompletedTask;
	}

	public Task<JobInfo?> GetJob(string jobIdentifier)
	{
		if (_jobDictionary.TryGetValue(jobIdentifier, out var jobInfo))
			return Task.FromResult<JobInfo?>(jobInfo);

		return Task.FromResult<JobInfo?>(null);
	}
	public Task<IEnumerable<JobInfo>> GetJobs() => Task.FromResult(_jobDictionary.Values.AsEnumerable());


	public Task Register(JobInfo jobInfo)
	{
		_jobDictionary.Add(jobInfo.Identifier, jobInfo);
		return Task.CompletedTask;
	}

	public Task<AccessState> RequestAccess() => Task.FromResult(AccessState.Available);

	public async Task<JobRunResult> Run(string jobIdentifier, CancellationToken cancelToken = default)
	{
		var jobInfo = _jobDictionary[jobIdentifier];
		var jobType = jobInfo.Type;

		var job = (IJob)(Activator.CreateInstance(jobType) ?? throw new NullReferenceException());

		try
		{
			await job.Run(jobInfo, cancelToken);
		}
		catch (Exception e)
		{
			return new JobRunResult(jobInfo, e);
		}

		return new JobRunResult(jobInfo, null);
	}

	public async Task<IEnumerable<JobRunResult>> RunAll(CancellationToken cancelToken = default, bool runSequentially = false)
	{
		if (runSequentially)
		{
			var resultList = new List<JobRunResult>();

			foreach (var job in _jobDictionary)
			{
				var result = await Run(job.Key, CancellationToken.None);
				resultList.Add(result);
			}

			return resultList;
		}
		else
		{
			return await Task.WhenAll(_jobDictionary.Select(x => Run(x.Key)));
		}
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