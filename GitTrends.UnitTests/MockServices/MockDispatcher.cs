namespace GitTrends.UnitTests;

class MockDispatcher : IDispatcher
{
	public bool Dispatch(Action action) => true;
	public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotImplementedException();
	public IDispatcherTimer CreateTimer() => throw new NotImplementedException();
	public bool IsDispatchRequired { get; } = false;
}