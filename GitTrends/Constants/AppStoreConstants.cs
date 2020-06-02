using System;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public static class AppStoreConstants
    {
        const string _appStoreRatingRequest_iOS = "How about a review on the App Store, then?";
        const string _appStoreRatingRequest_Android = "How about a review on the Google Play Store, then?";
        const string _appStoreRatingRequest_Windows = "How about a review on the Windows Store, then?";
        const string _appStoreRatingRequest_Other = "How about a posting a review, then?";

        const string _appStoreLink = "itms://apps.apple.com/app/gittrends-github-insights/id1500300399?action=write-review";
        const string _googlePlayStoreLink = "market://details?id=com.minnick.gittrends";

        const string _appleAppStoreUrl = "https://apps.apple.com/app/gittrends-github-insights/id1500300399?action=write-review";
        const string _googlePlayStoreUrl = "https://play.google.com/store/apps/details?id=com.minnick.gittrends";

        const string _placeHolderUrl = "https://gittrends.com";

        public static string RatingRequest { get; } = Device.RuntimePlatform switch
        {
            Device.iOS => _appStoreRatingRequest_iOS,
            Device.Android => _appStoreRatingRequest_Android,
            Device.UWP => _appStoreRatingRequest_Windows,
            Device.GTK => throw new NotImplementedException(),
            Device.macOS => throw new NotImplementedException(),
            Device.WPF => throw new NotImplementedException(),
            Device.Tizen => throw new NotImplementedException(),
            _ => _appStoreRatingRequest_Other
        };

        public static string AppLink { get; } = Device.RuntimePlatform switch
        {
            Device.iOS => _appStoreLink,
            Device.Android => _googlePlayStoreLink,
            Device.UWP => throw new NotImplementedException(),
            Device.GTK => throw new NotImplementedException(),
            Device.macOS => throw new NotImplementedException(),
            Device.WPF => throw new NotImplementedException(),
            Device.Tizen => throw new NotImplementedException(),
            _ => _placeHolderUrl
        };

        public static string Url { get; } = Device.RuntimePlatform switch
        {
            Device.iOS => _appleAppStoreUrl,
            Device.Android => _googlePlayStoreUrl,
            Device.UWP => throw new NotImplementedException(),
            Device.GTK => throw new NotImplementedException(),
            Device.macOS => throw new NotImplementedException(),
            Device.WPF => throw new NotImplementedException(),
            Device.Tizen => throw new NotImplementedException(),
            _ => _placeHolderUrl
        };
    }
}
