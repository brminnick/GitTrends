using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class ReferringSitesViewModel : BaseViewModel
    {
        readonly static WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new WeakEventManager<PullToRefreshFailedEventArgs>();

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
        string _emptyDataViewDescription = string.Empty;

        bool _isRefreshing, _isStoreRatingRequestVisible, _isEmptyDataViewEnabled;

        public ReferringSitesViewModel(IMainThread mainThread,
                                        ReviewService reviewService,
                                        FavIconService favIconService,
                                        IVersionTracking versionTracking,
                                        IAnalyticsService analyticsService,
                                        GitHubUserService gitHubUserService,
                                        GitHubApiV3Service gitHubApiV3Service,
                                        DeepLinkingService deepLinkingService,
                                        ReferringSitesDatabase referringSitesDatabase,
                                        GitHubAuthenticationService gitHubAuthenticationService) : base(analyticsService, mainThread)
        {
            ReviewService.ReviewRequested += HandleReviewRequested;
            ReviewService.ReviewCompleted += HandleReviewCompleted;

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

        public static event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }

        public ICommand NoButtonCommand { get; }
        public ICommand YesButtonCommand { get; }
        public IAsyncCommand<(string Owner, string Repository, string RepositoryUrl, CancellationToken Token)> RefreshCommand { get; }

        public string EmptyDataViewTitle
        {
            get => _emptyDataViewText;
            set => SetProperty(ref _emptyDataViewText, value);
        }

        public string EmptyDataViewDescription
        {
            get => _emptyDataViewDescription;
            set => SetProperty(ref _emptyDataViewDescription, value);
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
            get => _mobileReferringSiteList ??= new List<MobileReferringSiteModel>();
            set => SetProperty(ref _mobileReferringSiteList, value);
        }

        RefreshState RefreshState
        {
            set
            {
                EmptyDataViewTitle = EmptyDataViewService.GetReferringSitesTitleText(value);
                EmptyDataViewDescription = EmptyDataViewService.GetReferringSitesDescriptionText(value);
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
            HttpResponseMessage? finalResponse = null;
            IReadOnlyList<ReferringSiteModel> referringSitesList = new List<ReferringSiteModel>();

            try
            {
                referringSitesList = await _gitHubApiV3Service.GetReferringSites(owner, repository, cancellationToken).ConfigureAwait(false);

                MobileReferringSitesList = MobileSortingService.SortReferringSites(referringSitesList.Select(x => new MobileReferringSiteModel(x))).ToList();

                if (!_gitHubUserService.IsDemoUser)
                {
                    //Call EnsureSuccessStatusCode to confirm the above API calls executed successfully
                    finalResponse = await _gitHubApiV3Service.GetGitHubApiResponse(cancellationToken).ConfigureAwait(false);
                    finalResponse.EnsureSuccessStatusCode();
                }

                RefreshState = RefreshState.Succeeded;
            }
            catch (Exception e) when ((e is ApiException exception && exception.StatusCode is HttpStatusCode.Unauthorized)
                                        || (e is HttpRequestException && finalResponse != null && finalResponse.StatusCode is HttpStatusCode.Unauthorized))
            {
                OnPullToRefreshFailed(new LoginExpiredPullToRefreshEventArgs());

                await _gitHubAuthenticationService.LogOut().ConfigureAwait(false);

                RefreshState = RefreshState.LoginExpired;
            }
            catch (Exception e) when (GitHubApiService.HasReachedMaximimApiCallLimit(e)
                                        || (e is HttpRequestException && finalResponse != null && GitHubApiService.HasReachedMaximimApiCallLimit(finalResponse.Headers)))
            {
                var responseHeaders = e switch
                {
                    ApiException exception => exception.Headers,
                    HttpRequestException _ when finalResponse != null => finalResponse.Headers,
                    _ => throw new NotSupportedException()
                };

                OnPullToRefreshFailed(new MaximimApiRequestsReachedEventArgs(GitHubApiService.GetRateLimitResetDateTime(responseHeaders)));

                RefreshState = RefreshState.MaximumApiLimit;
            }
            catch (Exception e)
            {
                OnPullToRefreshFailed(new ErrorPullToRefreshEventArgs(ReferringSitesPageConstants.ErrorPullToRefreshEventArgs));

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

                if (_gitHubUserService.IsDemoUser)
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
            _pullToRefreshFailedEventManager.RaiseEvent(this, pullToRefreshFailedEventArgs, nameof(PullToRefreshFailed));
    }
}
