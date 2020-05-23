using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace GitTrends
{
    public class MobileReferringSiteModel : ReferringSiteModel, INotifyPropertyChanged, IMobileReferringSiteModel
    {
        public const int FavIconSize = 32;

        readonly WeakEventManager _propertyChangedEventManager = new WeakEventManager();

        ImageSource? _favIcon;

        public MobileReferringSiteModel(in ReferringSiteModel referringSiteModel, in ImageSource? favIcon = null)
            : base(referringSiteModel.TotalCount, referringSiteModel.TotalUniqueCount, referringSiteModel.Referrer, referringSiteModel.DownloadedAt)
        {
            FavIcon = favIcon;
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => _propertyChangedEventManager.AddEventHandler(value);
            remove => _propertyChangedEventManager.RemoveEventHandler(value);
        }

        public string FavIconImageUrl => FavIcon switch
        {
            UriImageSource uriImageSource => uriImageSource.Uri.ToString(),
            _ => string.Empty
        };

        [JsonIgnore]
        public ImageSource? FavIcon
        {
            get => _favIcon;
            set => SetProperty(ref _favIcon, value);
        }

        protected void SetProperty<T>(ref T backingStore, in T value, [CallerMemberName] in string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return;

            backingStore = value;

            _propertyChangedEventManager.HandleEvent(this, new PropertyChangedEventArgs(propertyName), nameof(INotifyPropertyChanged.PropertyChanged));
        }
    }
}
