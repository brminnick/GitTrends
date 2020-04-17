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

                return new MobileReferringSiteModel(referringSiteModel);
            }
        }
    }
}
