using CommunityToolkit.Mvvm.ComponentModel;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	[INotifyPropertyChanged]
	public abstract partial class BaseViewModel
	{
		public BaseViewModel(IAnalyticsService analyticsService, IMainThread mainThread) =>
			(AnalyticsService, MainThread) = (analyticsService, mainThread);

		protected IAnalyticsService AnalyticsService { get; }
		protected IMainThread MainThread { get; }
	}
}