using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    class ReviewService
    {
        readonly WeakEventManager<ReviewRequest> _reviewCompletedEventManager = new WeakEventManager<ReviewRequest>();
        readonly WeakEventManager _reviewPromptRequestedEventManager = new WeakEventManager();

        readonly AnalyticsService _analyticsService;

        public ReviewService(AnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;

            if (AppInstallDate == default)
                Preferences.Set(nameof(AppInstallDate), DateTime.UtcNow);
        }

        public event EventHandler ReviewRequested
        {
            add => _reviewPromptRequestedEventManager.AddEventHandler(value);
            remove => _reviewPromptRequestedEventManager.RemoveEventHandler(value);
        }

        public event EventHandler<ReviewRequest> ReviewCompleted
        {
            add => _reviewCompletedEventManager.AddEventHandler(value);
            remove => _reviewCompletedEventManager.RemoveEventHandler(value);
        }

        public string StoreRatingRequestViewTitle => CurrentState switch
        {
            ReviewState.Greeting => string.IsNullOrWhiteSpace(MostRecentReviewedBuildString) ? ReviewServiceConstants.TitleLabel_EnjoyingGitTrends : ReviewServiceConstants.TitleLabel_EnjoyingNewVersionOfGitTrends,
            ReviewState.RequestFeedback => ReviewServiceConstants.TitleLabel_Feedback,
            ReviewState.RequestReview => ReviewServiceConstants.TitleLabel_AppStoreRatingRequest,
            _ => throw new NotSupportedException()
        };

        public string YesButtonText => CurrentState switch
        {
            ReviewState.Greeting => ReviewServiceConstants.YesButton_Yes,
            ReviewState.RequestFeedback => ReviewServiceConstants.YesButton_OkSure,
            ReviewState.RequestReview => ReviewServiceConstants.YesButton_OkSure,
            _ => throw new NotSupportedException()
        };

        public string NoButtonText => CurrentState switch
        {
            ReviewState.Greeting => ReviewServiceConstants.NoButton_NotReally,
            ReviewState.RequestFeedback => ReviewServiceConstants.NoButton_NoThanks,
            ReviewState.RequestReview => ReviewServiceConstants.NoButton_NoThanks,
            _ => throw new NotSupportedException()
        };

        public ReviewState CurrentState { get; private set; } = ReviewState.Greeting;

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

        public void UpdateState(in ReviewAction action)
        {
            var previousState = CurrentState;

            var updatedState = action switch
            {
                ReviewAction.NoButtonTapped when CurrentState is ReviewState.Greeting => ReviewState.RequestFeedback,
                ReviewAction.NoButtonTapped => ReviewState.Greeting,
                ReviewAction.YesButtonTapped when CurrentState is ReviewState.Greeting => ReviewState.RequestReview,
                ReviewAction.YesButtonTapped => ReviewState.Greeting,
                _ => throw new NotSupportedException()
            };

            CurrentState = updatedState;

            if (action is ReviewAction.YesButtonTapped && previousState is ReviewState.RequestReview)
                OnReviewRequestCompleted(ReviewRequest.AppStore);
            else if (action is ReviewAction.YesButtonTapped && previousState is ReviewState.RequestFeedback)
                OnReviewRequestCompleted(ReviewRequest.Email);
            else if (previousState is ReviewState.RequestReview || previousState is ReviewState.RequestFeedback)
                OnReviewRequestCompleted(ReviewRequest.None);
        }

        public void TryRequestReviewPrompt()
        {
            if (ShouldDisplayReviewRequest())
            {
                _analyticsService.Track("Review Request Triggered", nameof(Device.RuntimePlatform), Device.RuntimePlatform);

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

        void OnReviewRequested() => _reviewPromptRequestedEventManager.HandleEvent(this, EventArgs.Empty, nameof(ReviewRequested));
        void OnReviewRequestCompleted(ReviewRequest reviewRequested) => _reviewCompletedEventManager.HandleEvent(this, reviewRequested, nameof(ReviewCompleted));
    }

    enum ReviewState { Greeting, RequestFeedback, RequestReview }
    enum ReviewAction { NoButtonTapped, YesButtonTapped }
    enum ReviewRequest { None, AppStore, Email }

    public interface ISKStoreReviewController
    {
        Task RequestReview();
    }
}
