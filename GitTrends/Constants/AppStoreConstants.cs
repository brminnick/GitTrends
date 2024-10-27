using GitTrends.Mobile.Common.Constants;

namespace GitTrends;

public class AppStoreConstants(IDeviceInfo deviceInfo)
{
	const string _appStoreLink = "itms://apps.apple.com/app/gittrends-github-insights/id1500300399?action=write-review";
	const string _googlePlayStoreLink = "market://details?id=com.minnick.gittrends";

	const string _appleAppStoreUrl = "https://apps.apple.com/app/gittrends-github-insights/id1500300399?action=write-review";
	const string _googlePlayStoreUrl = "https://play.google.com/store/apps/details?id=com.minnick.gittrends";

	const string _placeHolderUrl = "https://gittrends.com";

	readonly IDeviceInfo _deviceInfo = deviceInfo;

	public string RatingRequest
	{
		get
		{
			if (_deviceInfo.Platform == DevicePlatform.iOS)
				return AppStoreRatingRequestConstants.iOS;

			if (_deviceInfo.Platform == DevicePlatform.Android)
				return AppStoreRatingRequestConstants.Android;

			if (_deviceInfo.Platform == DevicePlatform.WinUI)
				throw new NotImplementedException();

			if (_deviceInfo.Platform == DevicePlatform.watchOS)
				throw new NotImplementedException();

			if (_deviceInfo.Platform == DevicePlatform.tvOS)
				throw new NotImplementedException();

			if (_deviceInfo.Platform == DevicePlatform.Tizen)
				throw new NotImplementedException();

			return AppStoreRatingRequestConstants.Other;
		}
	}

	public string AppLink
	{
		get
		{
			if (_deviceInfo.Platform == DevicePlatform.iOS)
				return _appStoreLink;

			if (_deviceInfo.Platform == DevicePlatform.Android)
				return _googlePlayStoreLink;

			if (_deviceInfo.Platform == DevicePlatform.WinUI)
				throw new NotImplementedException();

			if (_deviceInfo.Platform == DevicePlatform.watchOS)
				throw new NotImplementedException();

			if (_deviceInfo.Platform == DevicePlatform.tvOS)
				throw new NotImplementedException();

			if (_deviceInfo.Platform == DevicePlatform.Tizen)
				throw new NotImplementedException();

			return _placeHolderUrl;
		}
	}

	public string Url
	{
		get
		{
			if (_deviceInfo.Platform == DevicePlatform.iOS)
				return _appleAppStoreUrl;

			if (_deviceInfo.Platform == DevicePlatform.Android)
				return _googlePlayStoreUrl;

			if (_deviceInfo.Platform == DevicePlatform.WinUI)
				throw new NotImplementedException();

			if (_deviceInfo.Platform == DevicePlatform.watchOS)
				throw new NotImplementedException();

			if (_deviceInfo.Platform == DevicePlatform.tvOS)
				throw new NotImplementedException();

			if (_deviceInfo.Platform == DevicePlatform.Tizen)
				throw new NotImplementedException();

			return _placeHolderUrl;
		}
	}
}