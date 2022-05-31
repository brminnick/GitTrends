using System.Runtime.CompilerServices;
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

		protected bool SetProperty<T>(ref T field, in T newValue, in System.Action? onChanged = null, [CallerMemberName] in string? propertyName = null)
		{
			var didPropertyChange = SetProperty(ref field, newValue, propertyName);

			if (didPropertyChange)
				onChanged?.Invoke();

			return didPropertyChange;
		}
	}
}