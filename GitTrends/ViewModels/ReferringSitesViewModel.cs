using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    class ReferringSitesViewModel : BaseViewModel
    {
        readonly WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new WeakEventManager<PullToRefreshFailedEventArgs>();

        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly DeepLinkingService _deepLinkingService;
        readonly ReviewService _reviewService;
        readonly GitHubAuthenticationService _gitHubAuthenticationService;

        IReadOnlyList<MobileReferringSiteModel>? _mobileReferringSiteList;

        string _reviewRequestView_NoButtonText = string.Empty;
        string _reviewRequestView_YesButtonText = string.Empty;
        string _reviewRequestView_TitleLabel = string.Empty;

        bool _isRefreshing, _isStoreRatingRequestVisible, _isEmptyDataViewEnabled;

        public ReferringSitesViewModel(GitHubApiV3Service gitHubApiV3Service,
                                        DeepLinkingService deepLinkingService,
                                        AnalyticsService analyticsService,
                                        GitHubAuthenticationService gitHubAuthenticationService,
                                        ReviewService reviewService) : base(analyticsService)
        {
            reviewService.ReviewRequested += HandleReviewRequested;
            reviewService.ReviewCompleted += HandleReviewCompleted;

            _reviewService = reviewService;
            _gitHubApiV3Service = gitHubApiV3Service;
            _deepLinkingService = deepLinkingService;
            _gitHubAuthenticationService = gitHubAuthenticationService;

            RefreshCommand = new AsyncCommand<(string Owner, string Repository, CancellationToken Token)>(tuple => ExecuteRefreshCommand(tuple.Owner, tuple.Repository, tuple.Token));
            NoButtonCommand = new Command(() => HandleReviewRequestButtonTapped(ReviewAction.NoButtonTapped));
            YesButtonCommand = new Command(() => HandleReviewRequestButtonTapped(ReviewAction.YesButtonTapped));

            UpdateStoreRatingRequestView();
        }

        public event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }

        public ICommand NoButtonCommand { get; }
        public ICommand YesButtonCommand { get; }
        public ICommand RefreshCommand { get; }

        public bool IsEmptyDataViewEnabled
        {
            get => _isEmptyDataViewEnabled;
            set => SetProperty(ref _isEmptyDataViewEnabled, value);
        }

        public string ReviewRequestView_TitleLabel
        {
            get => _reviewRequestView_TitleLabel;
            set => SetProperty(ref _reviewRequestView_TitleLabel, value);
        }

        public string ReviewRequestView_NoButtonText
        {
            get => _reviewRequestView_NoButtonText;
            set => SetProperty(ref _reviewRequestView_NoButtonText, value);
        }

        public string ReviewRequestView_YesButtonText
        {
            get => _reviewRequestView_YesButtonText;
            set => SetProperty(ref _reviewRequestView_YesButtonText, value);
        }

        public bool IsStoreRatingRequestVisible
        {
            get => _isStoreRatingRequestVisible;
            set => SetProperty(ref _isStoreRatingRequestVisible, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public IReadOnlyList<MobileReferringSiteModel> MobileReferringSitesList
        {
            get => _mobileReferringSiteList ??= Enumerable.Empty<MobileReferringSiteModel>().ToList();
            set => SetProperty(ref _mobileReferringSiteList, value);
        }

        void HandleReviewRequestButtonTapped(in ReviewAction action)
        {
            AnalyticsService.Track("Review Request Button Tapped", new Dictionary<string, string>
            {
                { nameof(ReviewAction), action.ToString() },
                { nameof(ReviewService.CurrentState),  _reviewService.CurrentState.ToString() }
            });

            _reviewService.UpdateState(action);
            UpdateStoreRatingRequestView();
        }

        void UpdateStoreRatingRequestView()
        {
            ReviewRequestView_TitleLabel = _reviewService.StoreRatingRequestViewTitle;
            ReviewRequestView_NoButtonText = _reviewService.NoButtonText;
            ReviewRequestView_YesButtonText = _reviewService.YesButtonText;
        }

        async Task ExecuteRefreshCommand(string owner, string repository, CancellationToken cancellationToken)
        {
            IReadOnlyList<ReferringSiteModel> referringSitesList = Enumerable.Empty<ReferringSiteModel>().ToList();

            try
            {
                referringSitesList = await _gitHubApiV3Service.GetReferringSites(owner, repository, cancellationToken).ConfigureAwait(false);

                MobileReferringSitesList = SortingService.SortReferringSites(referringSitesList.Select(x => new MobileReferringSiteModel(x))).ToList();
            }
            catch (ApiException e) when (e.StatusCode is HttpStatusCode.Unauthorized)
            {
                OnPullToRefreshFailed(new LoginExpiredPullToRefreshEventArgs());

                await _gitHubAuthenticationService.LogOut().ConfigureAwait(false);
            }
            catch (ApiException e) when (GitHubApiService.HasReachedMaximimApiCallLimit(e))
            {
                OnPullToRefreshFailed(new MaximimApiRequestsReachedEventArgs(GitHubApiService.GetRateLimitResetDateTime(e)));
            }
            catch (Exception e)
            {
                AnalyticsService.Report(e);
                await _deepLinkingService.DisplayAlert("Error", "Unable to retrieve referring sites. Check your internet connection and try again.", "OK").ConfigureAwait(false);
            }
            finally
            {
                IsEmptyDataViewEnabled = true;
            }

            try
            {
                await foreach (var mobileReferringSite in GetMobileReferringSiteWithFavIconList(referringSitesList, cancellationToken).ConfigureAwait(false))
                {
                    var referringSite = MobileReferringSitesList.Single(x => x.Referrer == mobileReferringSite.Referrer);
                    referringSite.FavIcon = mobileReferringSite.FavIcon;
                }
            }
            catch (Exception e)
            {
                //Let's track the exception, but we don't need to do anything with it because the data still appears, just withoutthe icons
                AnalyticsService.Report(e);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        async IAsyncEnumerable<MobileReferringSiteModel> GetMobileReferringSiteWithFavIconList(IEnumerable<ReferringSiteModel> referringSites, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var favIconTaskList = referringSites.Select(x => setFavIcon(x, cancellationToken)).ToList();

            while (favIconTaskList.Any())
            {
                var completedFavIconTask = await Task.WhenAny(favIconTaskList).ConfigureAwait(false);
                favIconTaskList.Remove(completedFavIconTask);

                var mobileReferringSiteModel = await completedFavIconTask.ConfigureAwait(false);
                yield return mobileReferringSiteModel;
            }

            static async Task<MobileReferringSiteModel> setFavIcon(ReferringSiteModel referringSiteModel, CancellationToken cancellationToken)
            {
                if (referringSiteModel.ReferrerUri != null && referringSiteModel.IsReferrerUriValid)
                {
                    var favIcon = await FavIconService.GetFavIconImageSource(referringSiteModel.ReferrerUri, cancellationToken).ConfigureAwait(false);
                    return new MobileReferringSiteModel(referringSiteModel, favIcon);
                }
                else
                {
                    return new MobileReferringSiteModel(referringSiteModel, FavIconService.DefaultFavIcon);
                }
            }
        }

        async void HandleReviewCompleted(object sender, ReviewRequest request)
        {
            AnalyticsService.Track("Review Completed", nameof(ReviewRequest), request.ToString());

            switch (request)
            {
                case ReviewRequest.AppStore:
                    await _deepLinkingService.OpenApp(AppStoreConstants.AppLink, AppStoreConstants.Url).ConfigureAwait(false);
                    break;

                case ReviewRequest.Email:
                    await _deepLinkingService.SendEmail($"GitTrends App Feedback, Version {VersionTracking.CurrentVersion}",
                                                        "Here's my feedback on how to make the app great!",
                                                        new[] { "support@gittrends.com" }).ConfigureAwait(false);
                    break;

                case ReviewRequest.None:
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        void HandleReviewRequested(object sender, EventArgs e) => IsStoreRatingRequestVisible = true;

        void OnPullToRefreshFailed(PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs) =>
            _pullToRefreshFailedEventManager.HandleEvent(this, pullToRefreshFailedEventArgs, nameof(PullToRefreshFailed));
    }
}
