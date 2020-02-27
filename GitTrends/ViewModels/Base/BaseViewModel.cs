using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;

namespace GitTrends
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        readonly WeakEventManager _propertyChangedEventManager = new WeakEventManager();

        public BaseViewModel(AnalyticsService analyticsService) => AnalyticsService = analyticsService;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => _propertyChangedEventManager.AddEventHandler(value);
            remove => _propertyChangedEventManager.RemoveEventHandler(value);
        }

        protected AnalyticsService AnalyticsService { get; }

        protected void SetProperty<T>(ref T backingStore, in T value, System.Func<Task> onChanged, [CallerMemberName] in string propertyname = "") =>
            SetProperty(ref backingStore, value, async () => await onChanged().ConfigureAwait(false), propertyname);

        protected void SetProperty<T>(ref T backingStore, in T value, in System.Action? onChanged = null, [CallerMemberName] in string propertyname = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return;

            backingStore = value;

            onChanged?.Invoke();

            OnPropertyChanged(propertyname);
        }

        protected void OnPropertyChanged([CallerMemberName] in string propertyName = "") =>
            _propertyChangedEventManager.HandleEvent(this, new PropertyChangedEventArgs(propertyName), nameof(INotifyPropertyChanged.PropertyChanged));
    }
}