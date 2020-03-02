using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    class ReviewService
    {
        readonly WeakEventManager _reviewRequestedEventManager = new WeakEventManager();
        readonly AnalyticsService _analyticsService;

        public ReviewService(AnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;

            if (AppInstallDate == default)
                Preferences.Set(nameof(AppInstallDate), DateTime.UtcNow);
        }

        public event EventHandler ReviewRequested
        {
            add => _reviewRequestedEventManager.AddEventHandler(value);
            remove => _reviewRequestedEventManager.RemoveEventHandler(value);
        }

        DateTime AppInstallDate => Preferences.Get(nameof(AppInstallDate), default(DateTime));

        int ReviewRequests
        {
            get => Preferences.Get(nameof(ReviewRequests), 0);
            set => Preferences.Set(nameof(ReviewRequests), value);
        }

        DateTime MostRecentRequestDate
        {
            get => Preferences.Get(nameof(MostRecentRequestDate), default(DateTime));
            set => Preferences.Set(nameof(MostRecentRequestDate), value);
        }

        string MostRecentReviewedBuildString
        {
            get => Preferences.Get(nameof(MostRecentReviewedBuildString), string.Empty);
            set => Preferences.Set(nameof(MostRecentReviewedBuildString), value);
        }

        public void TryRequestReview()
        {
            if (ShouldDisplayReviewRequest())
            {
                if (Device.RuntimePlatform is Device.iOS)
                    DependencyService.Get<ISKStoreReviewController>().RequestReview().SafeFireAndForget(ex => _analyticsService.Report(ex));
                else
                    OnReviewRequested();

                MostRecentReviewedBuildString = AppInfo.BuildString;
                MostRecentRequestDate = DateTime.UtcNow;
            }
            else
            {
                ReviewRequests++;
            }
        }

        bool ShouldDisplayReviewRequest()
        {
            return ReviewRequests > 20
                    && MostRecentReviewedBuildString != AppInfo.BuildString
                    && DateTime.Compare(AppInstallDate.Add(TimeSpan.FromDays(14)), DateTime.UtcNow) < 1
                    && DateTime.Compare(MostRecentRequestDate.Add(TimeSpan.FromDays(90)), DateTime.UtcNow) < 1;
        }

        void OnReviewRequested() => _reviewRequestedEventManager.HandleEvent(this, EventArgs.Empty, nameof(ReviewRequested));
    }

    public interface ISKStoreReviewController
    {
        Task RequestReview();
    }
}
