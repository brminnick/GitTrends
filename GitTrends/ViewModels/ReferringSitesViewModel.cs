using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
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
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly DeepLinkingService _deepLinkingService;
        readonly ReviewService _reviewService;

        IReadOnlyList<MobileReferringSiteModel> _mobileReferringSiteList = Enumerable.Empty<MobileReferringSiteModel>().ToList();

        bool _isRefreshing;

        string _reviewRequestView_NoButtonText = string.Empty;
        string _reviewRequestView_YesButtonText = string.Empty;
        string _reviewRequestView_TitleLabel = string.Empty;
#if DEBUG
        bool _isStoreRatingRequestVisible = true;
#else
        bool _isStoreRatingRequestVisible = false;
#endif

        public ReferringSitesViewModel(GitHubApiV3Service gitHubApiV3Service,
                                        DeepLinkingService deepLinkingService,
                                        AnalyticsService analyticsService,
                                        ReviewService reviewService) : base(analyticsService)
        {
            reviewService.ReviewRequested += HandleReviewRequested;
            reviewService.ReviewCompleted += HandleReviewCompleted;

            _reviewService = reviewService;
            _gitHubApiV3Service = gitHubApiV3Service;
            _deepLinkingService = deepLinkingService;

            RefreshCommand = new AsyncCommand<(string Owner, string Repository)>(repo => ExecuteRefreshCommand(repo.Owner, repo.Repository));
            NoButtonCommand = new Command(() => HandleReviewRequestButtonTapped(ReviewAction.NoButtonTapped));
            YesButtonCommand = new Command(() => HandleReviewRequestButtonTapped(ReviewAction.YesButtonTapped));

            UpdateStoreRatingRequestView();
        }

        public ICommand NoButtonCommand { get; }
        public ICommand YesButtonCommand { get; }
        public ICommand RefreshCommand { get; }

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
            get => _mobileReferringSiteList;
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

        async Task ExecuteRefreshCommand(string owner, string repository)
        {
            try
            {
                var referringSitesList = await _gitHubApiV3Service.GetReferringSites(owner, repository).ConfigureAwait(false);

                if (!referringSitesList.Any())
                {
                    await _deepLinkingService.DisplayAlert(ReferringSitesConstants.NoReferringSitesTitle, ReferringSitesConstants.NoReferringSitesDescription, ReferringSitesConstants.NoReferringSitesOK).ConfigureAwait(false);
                }
                else
                {
                    MobileReferringSitesList = SortingService.SortReferringSites(referringSitesList.Select(x => new MobileReferringSiteModel(x))).ToList();

                    await foreach (var mobileReferringSite in GetMobileReferringSiteWithFavIconList(referringSitesList).ConfigureAwait(false))
                    {
                        var referringSite = MobileReferringSitesList.Single(x => x.Referrer == mobileReferringSite.Referrer);
                        referringSite.FavIcon = mobileReferringSite.FavIcon;
                    }
                }
            }
            catch (ApiException e) when (e.StatusCode is HttpStatusCode.Unauthorized)
            {
                await _deepLinkingService.DisplayAlert("Login Expired", "Please login again", "OK").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                AnalyticsService.Report(e);
                await _deepLinkingService.DisplayAlert("Error", "Unable to retrieve referring sites. Check your internet connection and try again.", "OK").ConfigureAwait(false);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        async IAsyncEnumerable<MobileReferringSiteModel> GetMobileReferringSiteWithFavIconList(IEnumerable<ReferringSiteModel> referringSites)
        {
            var favIconTaskList = referringSites.Select(x => setFavIcon(x)).ToList();

            while (favIconTaskList.Any())
            {
                var completedFavIconTask = await Task.WhenAny(favIconTaskList).ConfigureAwait(false);
                favIconTaskList.Remove(completedFavIconTask);

                var mobileReferringSiteModel = await completedFavIconTask.ConfigureAwait(false);
                yield return mobileReferringSiteModel;
            }

            static async Task<MobileReferringSiteModel> setFavIcon(ReferringSiteModel referringSiteModel)
            {
                if (referringSiteModel.ReferrerUri != null)
                {
                    var favIcon = await FavIconService.GetFavIconImageSource(referringSiteModel.ReferrerUri).ConfigureAwait(false);
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
                    await _deepLinkingService.OpenApp(ReviewServiceConstants.AppStoreAppLink, ReviewServiceConstants.AppStoreUrl).ConfigureAwait(false);
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
    }
}
