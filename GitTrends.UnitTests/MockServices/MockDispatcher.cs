namespace GitTrends.UnitTests;

class MockDispatcher : IDispatcher
{
	public bool Dispatch(Action action)
	{
		action();
		return true;
	}

	public bool IsDispatchRequired { get; } = false;

	public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotImplementedException();

	public IDispatcherTimer CreateTimer() => throw new NotImplementedException();
}