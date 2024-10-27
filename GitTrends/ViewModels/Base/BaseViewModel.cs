using CommunityToolkit.Mvvm.ComponentModel;
using GitTrends.Common;

namespace GitTrends;

public abstract class BaseViewModel(IAnalyticsService analyticsService, IDispatcher dispatcher) : ObservableObject
{
	protected IAnalyticsService AnalyticsService { get; } = analyticsService;
	protected IDispatcher Dispatcher { get; } = dispatcher;
}