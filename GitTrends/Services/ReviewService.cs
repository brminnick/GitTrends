using System;
using GitTrends.Shared;
using Plugin.StoreReview.Abstractions;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class ReviewService
    {
        public const int MinimumReviewRequests = 20;
        public const int MinimumAppInstallDays = 14;
        public const int MinimumMostRecentRequestDays = 90;

        readonly static AsyncAwaitBestPractices.WeakEventManager _reviewPromptRequestedEventManager = new();

        readonly IAppInfo _appInfo;
        readonly IStoreReview _storeReview;
        readonly IPreferences _preferences;
        readonly IAnalyticsService _analyticsService;

        public ReviewService(IAppInfo appInfo,
                                IStoreReview storeReview,
                                IPreferences preferences,
                                IAnalyticsService analyticsService)
        {
            _appInfo = appInfo;
            _storeReview = storeReview;
            _preferences = preferences;
            _analyticsService = analyticsService;

            if (AppInstallDate == default)
                preferences.Set(nameof(AppInstallDate), DateTime.UtcNow);
        }

        public static event EventHandler ReviewRequested
        {
            add => _reviewPromptRequestedEventManager.AddEventHandler(value);
            remove => _reviewPromptRequestedEventManager.RemoveEventHandler(value);
        }

        DateTime AppInstallDate => _preferences.Get(nameof(AppInstallDate), default(DateTime));

        int ReviewRequests
        {
            get => _preferences.Get(nameof(ReviewRequests), 0);
            set => _preferences.Set(nameof(ReviewRequests), value);
        }

        DateTime MostRecentRequestDate
        {
            get => _preferences.Get(nameof(MostRecentRequestDate), default(DateTime));
            set => _preferences.Set(nameof(MostRecentRequestDate), value);
        }

        string MostRecentReviewedBuildString
        {
            get => _preferences.Get(nameof(MostRecentReviewedBuildString), string.Empty);
            set => _preferences.Set(nameof(MostRecentReviewedBuildString), value);
        }

        public void TryRequestReviewPrompt()
        {
            if (ShouldDisplayReviewRequest())
            {
                _analyticsService.Track("Review Request Triggered", nameof(Device.RuntimePlatform), Device.RuntimePlatform);

#if AppStore
                _storeReview.RequestReview(false);
#else
                _storeReview.RequestReview(true);
#endif

                OnReviewRequested();

                MostRecentReviewedBuildString = _appInfo.BuildString;
                MostRecentRequestDate = DateTime.UtcNow;
            }
            else
            {
                ReviewRequests++;
            }
        }

        bool ShouldDisplayReviewRequest()
        {
            return ReviewRequests >= MinimumReviewRequests
                    && MostRecentReviewedBuildString != _appInfo.BuildString
                    && DateTime.Compare(AppInstallDate.Add(TimeSpan.FromDays(MinimumAppInstallDays)), DateTime.UtcNow) < 1
                    && DateTime.Compare(MostRecentRequestDate.Add(TimeSpan.FromDays(MinimumMostRecentRequestDays)), DateTime.UtcNow) < 1;
        }

        void OnReviewRequested() => _reviewPromptRequestedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(ReviewRequested));
    }
}
