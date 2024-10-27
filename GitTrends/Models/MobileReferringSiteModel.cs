using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using GitTrends.Common;

namespace GitTrends;

public record MobileReferringSiteModel : ReferringSiteModel, IMobileReferringSiteModel, INotifyPropertyChanged
{
	public const int FavIconSize = 32;

	readonly AsyncAwaitBestPractices.WeakEventManager _propertyChangedEventManager = new();

	ImageSource? _favIcon;

	public MobileReferringSiteModel(in ReferringSiteModel referringSiteModel, in ImageSource? favIcon = null)
		: base(referringSiteModel.TotalCount, referringSiteModel.TotalUniqueCount, referringSiteModel.Referrer)
	{
		DownloadedAt = referringSiteModel.DownloadedAt;
		FavIcon = favIcon ?? FavIconService.DefaultFavIcon;
	}

	event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
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

		_propertyChangedEventManager.RaiseEvent(this, new PropertyChangedEventArgs(propertyName), nameof(INotifyPropertyChanged.PropertyChanged));
	}
}