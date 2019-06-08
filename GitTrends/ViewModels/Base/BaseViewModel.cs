using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AsyncAwaitBestPractices;

namespace GitTrends
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        #region Constant Fields
        readonly WeakEventManager _propertyChangedEventManager = new WeakEventManager();
        #endregion

        #region Events
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => _propertyChangedEventManager.AddEventHandler(value);
            remove => _propertyChangedEventManager.RemoveEventHandler(value);
        }
        #endregion

        #region Methods
        protected void SetProperty<T>(ref T backingStore, T value, System.Action onChanged = null, [CallerMemberName] string propertyname = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return;

            backingStore = value;

            onChanged?.Invoke();

            OnPropertyChanged(propertyname);
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            _propertyChangedEventManager.HandleEvent(this, new PropertyChangedEventArgs(propertyName), nameof(INotifyPropertyChanged.PropertyChanged));
        #endregion
    }
}