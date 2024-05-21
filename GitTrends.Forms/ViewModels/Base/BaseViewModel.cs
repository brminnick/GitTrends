using CommunityToolkit.Mvvm.ComponentModel;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public abstract class BaseViewModel : ObservableObject
	{
		public BaseViewModel(IAnalyticsService analyticsService, IMainThread mainThread) =>
			(AnalyticsService, MainThread) = (analyticsService, mainThread);

		protected IAnalyticsService AnalyticsService { get; }
		protected IMainThread MainThread { get; }
	}
}