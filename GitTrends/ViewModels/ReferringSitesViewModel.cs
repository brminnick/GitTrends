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

namespace GitTrends
{
    class ReferringSitesViewModel : BaseViewModel
    {
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly DeepLinkingService _deepLinkingService;

        bool _isRefreshing;
        bool _isActivityIndicatorVisible;
        IReadOnlyList<MobileReferringSiteModel> _mobileReferringSiteList = Enumerable.Empty<MobileReferringSiteModel>().ToList();

        public ReferringSitesViewModel(GitHubApiV3Service gitHubApiV3Service,
                                        DeepLinkingService deepLinkingService,
                                        AnalyticsService analyticsService) : base(analyticsService)
        {
            _gitHubApiV3Service = gitHubApiV3Service;
            _deepLinkingService = deepLinkingService;

            RefreshCommand = new AsyncCommand<(string Owner, string Repository)>(repo => ExecuteRefreshCommand(repo.Owner, repo.Repository));
        }

        public ICommand RefreshCommand { get; }

        public bool IsActivityIndicatorVisible
        {
            get => _isActivityIndicatorVisible;
            set => SetProperty(ref _isActivityIndicatorVisible, value);
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

        async Task ExecuteRefreshCommand(string owner, string repository)
        {
            Task minimumActivityIndicatorTimeTask = Task.CompletedTask;

            //Only show the Activity Indicator when the page is first loaded
            if (!MobileReferringSitesList.Any())
            {
                IsActivityIndicatorVisible = true;
                minimumActivityIndicatorTimeTask = Task.Delay(1000);
            }

            try
            {
                var referringSitesList = await _gitHubApiV3Service.GetReferringSites(owner, repository).ConfigureAwait(false);

                if (!referringSitesList.Any())
                {
                    await _deepLinkingService.DisplayAlert(ReferringSitesConstants.NoReferringSitesTitle, ReferringSitesConstants.NoReferringSitesDescription, ReferringSitesConstants.NoReferringSitesOK).ConfigureAwait(false);
                    return;
                }

                //Begin retrieving favicon list before waiting for minimumActivityIndicatorTimeTask
                var getMobileReferringSiteWithFavIconListTask = GetMobileReferringSiteWithFavIconList(referringSitesList);

                await minimumActivityIndicatorTimeTask.ConfigureAwait(false);

                //Display the Referring Sites and hide the ActivityIndicator while FavIcons are still being retreived
                IsActivityIndicatorVisible = false;

                var mobileReferringSitesList_NoFavIcon = referringSitesList.Select(x => new MobileReferringSiteModel(x));
                displayMobileReferringSites(mobileReferringSitesList_NoFavIcon);

                try
                {
                    var mobileReferringSitesList_WithFavIcon = await getMobileReferringSiteWithFavIconListTask.ConfigureAwait(false);

                    //Display the Final Referring Sites with FavIcons
                    displayMobileReferringSites(mobileReferringSitesList_WithFavIcon);
                }
                catch (Exception e) when (!(e is ApiException apiException && apiException.StatusCode is HttpStatusCode.Unauthorized))
                {
                    //If the FavIcon fails to load, don't display an error dialog, unless the token has expired
                    AnalyticsService.Report(e);
                }
                finally
                {
                    IsRefreshing = false;
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
                IsActivityIndicatorVisible = IsRefreshing = false;
            }

            void displayMobileReferringSites(in IEnumerable<MobileReferringSiteModel> mobileReferringSiteList) =>
                MobileReferringSitesList = SortingService.SortReferringSites(mobileReferringSiteList).ToList();
        }

        async Task<List<MobileReferringSiteModel>> GetMobileReferringSiteWithFavIconList(List<ReferringSiteModel> referringSites)
        {
            var mobileReferringSiteList = new List<MobileReferringSiteModel>();

            var favIconTaskList = referringSites.Select(x => setFavIcon(x)).ToList();

            while (favIconTaskList.Any())
            {
                var completedFavIconTask = await Task.WhenAny(favIconTaskList).ConfigureAwait(false);
                favIconTaskList.Remove(completedFavIconTask);

                var mobileReferringSiteModel = await completedFavIconTask.ConfigureAwait(false);
                mobileReferringSiteList.Add(mobileReferringSiteModel);
            }

            return mobileReferringSiteList;

            static async Task<MobileReferringSiteModel> setFavIcon(ReferringSiteModel referringSiteModel)
            {
                if (referringSiteModel.ReferrerUri != null)
                {
                    var favIcon = await FavIconService.GetFavIconImageSource(referringSiteModel.ReferrerUri).ConfigureAwait(false);
                    return new MobileReferringSiteModel(referringSiteModel, favIcon);
                }

                return new MobileReferringSiteModel(referringSiteModel);
            }
        }
    }
}
