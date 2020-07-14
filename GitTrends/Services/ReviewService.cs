using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class ReviewService
    {
        public const int MinimumReviewRequests = 20;
        public const int MinimumAppInstallDays = 14;
        public const int MinimumMostRecentRequestDays = 90;

        readonly static WeakEventManager _reviewPromptRequestedEventManager = new WeakEventManager();
        readonly static WeakEventManager<ReviewRequest> _reviewCompletedEventManager = new WeakEventManager<ReviewRequest>();

        readonly IAppInfo _appInfo;
        readonly IPreferences _preferences;
        readonly IAnalyticsService _analyticsService;

        public ReviewService(IAnalyticsService analyticsService,
                                IPreferences preferences,
                                IAppInfo appInfo)
        {
            _appInfo = appInfo;
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

        public static event EventHandler<ReviewRequest> ReviewCompleted
        {
            add => _reviewCompletedEventManager.AddEventHandler(value);
            remove => _reviewCompletedEventManager.RemoveEventHandler(value);
        }

        public string StoreRatingRequestViewTitle => CurrentState switch
        {
            ReviewState.Greeting => string.IsNullOrWhiteSpace(MostRecentReviewedBuildString) ? ReviewServiceConstants.TitleLabel_EnjoyingGitTrends : ReviewServiceConstants.TitleLabel_EnjoyingNewVersionOfGitTrends,
            ReviewState.RequestFeedback => ReviewServiceConstants.TitleLabel_Feedback,
            ReviewState.RequestReview => AppStoreConstants.RatingRequest,
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
        void OnReviewRequestCompleted(ReviewRequest reviewRequested) => _reviewCompletedEventManager.RaiseEvent(this, reviewRequested, nameof(ReviewCompleted));
    }

    public interface ISKStoreReviewController
    {
        Task RequestReview();
    }
}
