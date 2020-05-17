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
using Xamarin.Essentials.Interfaces;
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
        readonly ReferringSitesDatabase _referringSitesDatabase;
        readonly FavIconService _favIconService;
        readonly IVersionTracking _versionTracking;
        readonly GitHubUserService _gitHubUserService;

        IReadOnlyList<MobileReferringSiteModel>? _mobileReferringSiteList;

        string _reviewRequestView_NoButtonText = string.Empty;
        string _reviewRequestView_YesButtonText = string.Empty;
        string _reviewRequestView_TitleLabel = string.Empty;
        string _emptyDataViewText = string.Empty;

        bool _isRefreshing, _isStoreRatingRequestVisible, _isEmptyDataViewEnabled;

        public ReferringSitesViewModel(GitHubApiV3Service gitHubApiV3Service,
                                        DeepLinkingService deepLinkingService,
                                        IAnalyticsService analyticsService,
                                        ReferringSitesDatabase referringSitesDatabase,
                                        GitHubAuthenticationService gitHubAuthenticationService,
                                        ReviewService reviewService,
                                        FavIconService favIconService,
                                        IMainThread mainThread,
                                        IVersionTracking versionTracking,
                                        GitHubUserService gitHubUserService) : base(analyticsService, mainThread)
        {
            reviewService.ReviewRequested += HandleReviewRequested;
            reviewService.ReviewCompleted += HandleReviewCompleted;

            _reviewService = reviewService;
            _favIconService = favIconService;
            _versionTracking = versionTracking;
            _gitHubUserService = gitHubUserService;
            _gitHubApiV3Service = gitHubApiV3Service;
            _deepLinkingService = deepLinkingService;
            _referringSitesDatabase = referringSitesDatabase;
            _gitHubAuthenticationService = gitHubAuthenticationService;

            RefreshState = RefreshState.Uninitialized;

            RefreshCommand = new AsyncCommand<(string Owner, string Repository, string RepositoryUrl, CancellationToken Token)>(tuple => ExecuteRefreshCommand(tuple.Owner, tuple.Repository, tuple.RepositoryUrl, tuple.Token));
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

        public string EmptyDataViewText
        {
            get => _emptyDataViewText;
            set => SetProperty(ref _emptyDataViewText, value);
        }

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

        RefreshState RefreshState
        {
            set
            {
                const string swipeDownToRefresh = "\nSwipe down to retrieve referring sites";
                const string emptyList = "No referrals yet";
                const string loginExpired = "GitHub Login Expired\nPlease login again";
                const string uninitialized = "Data not gathered" + swipeDownToRefresh;

                EmptyDataViewText = value switch
                {
                    RefreshState.Uninitialized => uninitialized,
                    RefreshState.Succeeded => emptyList,
                    RefreshState.LoginExpired => loginExpired,
                    RefreshState.Error => EmptyDataView.UnableToRetrieveDataText + swipeDownToRefresh,
                    RefreshState.MaximumApiLimit => EmptyDataView.UnableToRetrieveDataText + swipeDownToRefresh,
                    _ => throw new NotSupportedException()
                };
            }
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

        async Task ExecuteRefreshCommand(string owner, string repository, string repositoryUrl, CancellationToken cancellationToken)
        {
            IReadOnlyList<ReferringSiteModel> referringSitesList = Enumerable.Empty<ReferringSiteModel>().ToList();

            try
            {
                referringSitesList = await _gitHubApiV3Service.GetReferringSites(owner, repository, cancellationToken).ConfigureAwait(false);

                MobileReferringSitesList = SortingService.SortReferringSites(referringSitesList.Select(x => new MobileReferringSiteModel(x))).ToList();

                RefreshState = RefreshState.Succeeded;
            }
            catch (ApiException e) when (e.StatusCode is HttpStatusCode.Unauthorized)
            {
                OnPullToRefreshFailed(new LoginExpiredPullToRefreshEventArgs());

                await _gitHubAuthenticationService.LogOut().ConfigureAwait(false);

                RefreshState = RefreshState.LoginExpired;
            }
            catch (ApiException e) when (GitHubApiService.HasReachedMaximimApiCallLimit(e))
            {
                OnPullToRefreshFailed(new MaximimApiRequestsReachedEventArgs(GitHubApiService.GetRateLimitResetDateTime(e)));

                RefreshState = RefreshState.MaximumApiLimit;
            }
            catch (Exception e)
            {
                OnPullToRefreshFailed(new ErrorPullToRefreshEventArgs("Unable to retrieve referring sites. Check your internet connection and try again."));

                AnalyticsService.Report(e);

                RefreshState = RefreshState.Error;
            }
            finally
            {
                IsEmptyDataViewEnabled = true;
            }

            try
            {
                await foreach (var mobileReferringSite in GetMobileReferringSiteWithFavIconList(referringSitesList, repositoryUrl, cancellationToken).ConfigureAwait(false))
                {
                    var referringSite = MobileReferringSitesList.Single(x => x.Referrer == mobileReferringSite.Referrer);
                    referringSite.FavIcon = mobileReferringSite.FavIcon;
                }

                if (!_gitHubUserService.IsDemoUser)
                {
                    foreach (var referringSite in MobileReferringSitesList)
                        await _referringSitesDatabase.SaveReferringSite(referringSite, repositoryUrl).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                //Let's track the exception, but we don't need to do anything with it because the data still appears, just without the icons
                AnalyticsService.Report(e);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        async IAsyncEnumerable<MobileReferringSiteModel> GetMobileReferringSiteWithFavIconList(IEnumerable<ReferringSiteModel> referringSites, string repositoryUrl, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var favIconTaskList = referringSites.Select(x => setFavIcon(_referringSitesDatabase, x, repositoryUrl, cancellationToken)).ToList();

            while (favIconTaskList.Any())
            {
                var completedFavIconTask = await Task.WhenAny(favIconTaskList).ConfigureAwait(false);
                favIconTaskList.Remove(completedFavIconTask);

                var mobileReferringSiteModel = await completedFavIconTask.ConfigureAwait(false);
                yield return mobileReferringSiteModel;
            }

            async Task<MobileReferringSiteModel> setFavIcon(ReferringSitesDatabase referringSitesDatabase, ReferringSiteModel referringSiteModel, string repositoryUrl, CancellationToken cancellationToken)
            {
                var mobileReferringSiteFromDatabase = await referringSitesDatabase.GetReferringSite(repositoryUrl, referringSiteModel.ReferrerUri).ConfigureAwait(false);

                if (mobileReferringSiteFromDatabase != null && isFavIconValid(mobileReferringSiteFromDatabase))
                    return mobileReferringSiteFromDatabase;

                if(_gitHubUserService.IsDemoUser)
                {
                    //Display the Activity Indicator to ensure consistent UX
                    await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                    return new MobileReferringSiteModel(referringSiteModel, FavIconService.DefaultFavIcon);
                }
                else if (referringSiteModel.ReferrerUri != null && referringSiteModel.IsReferrerUriValid)
                {
                    var favIcon = await _favIconService.GetFavIconImageSource(referringSiteModel.ReferrerUri, cancellationToken).ConfigureAwait(false);
                    return new MobileReferringSiteModel(referringSiteModel, favIcon);
                }
                else
                {
                    return new MobileReferringSiteModel(referringSiteModel, FavIconService.DefaultFavIcon);
                }

                static bool isFavIconValid(MobileReferringSiteModel mobileReferringSiteModel) => !string.IsNullOrWhiteSpace(mobileReferringSiteModel.FavIconImageUrl) && mobileReferringSiteModel.DownloadedAt.CompareTo(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(30))) > 0;
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
                    await _deepLinkingService.SendEmail($"GitTrends App Feedback, Version {_versionTracking.CurrentVersion}",
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
