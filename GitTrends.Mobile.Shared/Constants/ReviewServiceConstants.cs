using System;
using Xamarin.Essentials;

namespace GitTrends.Mobile.Shared
{
    static class ReviewServiceConstants
    {
        public const string TitleLabel_EnjoyingGitTrends = "Enjoying GitTrends?";
        public const string TitleLabel_EnjoyingNewVersionOfGitTrends = "Enjoing the New Version of GitTrends?";
        public const string TitleLabel_Feedback = "Would you mind giving us some feedback?";

        public const string NoButton_NoThanks = "No Thanks";
        public const string NoButton_NotReally = "Not Really";

        public const string YesButton_OkSure = "Ok, Sure!";
        public const string YesButton_Yes = "Yes!";

        const string _appStoreRatingRequest_iOS = "How about a rating on the App Store, then?";
        const string _appStoreRatingRequest_Android = "How about a rating on the Google Play Store, then?";
        const string _appStoreRatingRequest_Windows = "How about a rating on the Windows Store, then?";

        const string _appStoreLink = "itms://apps.apple.com/app/gittrends-github-insights/id1500300399?action=write-review";
        const string _googlePlayStoreLink = "market://details?id=com.minnick.gittrends";

        const string _appleAppStoreUrl = "https://apps.apple.com/app/gittrends-github-insights/id1500300399?action=write-review";
        const string _googlePlayStoreUrl = "https://play.google.com/store/apps/details?id=com.minnick.gittrends";

        public static string TitleLabel_AppStoreRatingRequest
        {
            get
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    return _appStoreRatingRequest_iOS;
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                    return _appStoreRatingRequest_Android;
                else if (DeviceInfo.Platform == DevicePlatform.UWP)
                    return _appStoreRatingRequest_Windows;
                else
                    throw new NotSupportedException();
            }
        }

        public static string AppStoreAppLink
        {
            get
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    return _appStoreLink;
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                    return _googlePlayStoreLink;
                else
                    throw new NotSupportedException();
            }
        }

        public static string AppStoreUrl
        {
            get
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    return _appleAppStoreUrl;
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                    return _googlePlayStoreUrl;
                else
                    throw new NotSupportedException();
            }
        }
    }

    enum ReviewState { Greeting, RequestFeedback, RequestReview }
    enum ReviewAction { NoButtonTapped, YesButtonTapped }
    enum ReviewRequest { None, AppStore, Email }
}
