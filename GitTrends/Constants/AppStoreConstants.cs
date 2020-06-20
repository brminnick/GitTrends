using System;
using GitTrends.Mobile.Common.Constants;
using Xamarin.Forms;

namespace GitTrends
{
    public static class AppStoreConstants
    {
        const string _appStoreLink = "itms://apps.apple.com/app/gittrends-github-insights/id1500300399?action=write-review";
        const string _googlePlayStoreLink = "market://details?id=com.minnick.gittrends";

        const string _appleAppStoreUrl = "https://apps.apple.com/app/gittrends-github-insights/id1500300399?action=write-review";
        const string _googlePlayStoreUrl = "https://play.google.com/store/apps/details?id=com.minnick.gittrends";

        const string _placeHolderUrl = "https://gittrends.com";

        public static string RatingRequest { get; } = Device.RuntimePlatform switch
        {
            Device.iOS => AppStoreRatingRequestConstants.iOS,
            Device.Android => AppStoreRatingRequestConstants.Android,
            Device.UWP => AppStoreRatingRequestConstants.Windows,
            Device.GTK => throw new NotImplementedException(),
            Device.macOS => throw new NotImplementedException(),
            Device.WPF => throw new NotImplementedException(),
            Device.Tizen => throw new NotImplementedException(),
            _ => AppStoreRatingRequestConstants.Other
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
