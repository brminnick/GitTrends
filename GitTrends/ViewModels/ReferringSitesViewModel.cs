using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;

namespace GitTrends
{
    class ReferringSitesViewModel : BaseViewModel
    {
        readonly GitHubApiV3Service _gitHubApiV3Service;

        bool _isRefreshing;
        bool _isActivityIndicatorVisible;
        IReadOnlyList<MobileReferringSiteModel> _mobileReferringSiteList = Enumerable.Empty<MobileReferringSiteModel>().ToList();

        public ReferringSitesViewModel(GitHubApiV3Service gitHubApiV3Service, AnalyticsService analyticsService) : base(analyticsService)
        {
            _gitHubApiV3Service = gitHubApiV3Service;
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
            Task minimumActivityIndicatorTimeTask;

            //Only show the Activity Indicator when the page is first loaded
            if (!MobileReferringSitesList.Any())
            {
                IsActivityIndicatorVisible = true;
                minimumActivityIndicatorTimeTask = Task.Delay(1000);
            }
            else
            {
                minimumActivityIndicatorTimeTask = Task.CompletedTask;
            }

            try
            {
                var referringSitesList = await _gitHubApiV3Service.GetReferringSites(owner, repository).ConfigureAwait(false);

                var mobileReferringSitesList_NoFavIcon = referringSitesList.Select(x => new MobileReferringSiteModel(x));

                //Begin retrieving favicon list before waiting for minimumActivityIndicatorTimeTask
                var getMobileReferringSiteWithFavIconListTask = GetMobileReferringSiteWithFavIconList(referringSitesList);

                await minimumActivityIndicatorTimeTask.ConfigureAwait(false);

                //Display the Referring Sites and hide the activity indicators while FavIcons are still being retreived
                IsActivityIndicatorVisible = false;
                displayMobileReferringSites(mobileReferringSitesList_NoFavIcon);

                var mobileReferringSitesList_WithFavIcon = await getMobileReferringSiteWithFavIconListTask.ConfigureAwait(false);

                //Display the Final Referring Sites with FavIcons
                displayMobileReferringSites(mobileReferringSitesList_WithFavIcon);
            }
            finally
            {
                IsActivityIndicatorVisible = IsRefreshing = false;
            }

            void displayMobileReferringSites(in IEnumerable<MobileReferringSiteModel> mobileReferringSiteList) => MobileReferringSitesList = mobileReferringSiteList.OrderByDescending(x => x.TotalCount).ThenByDescending(x => x.TotalUniqueCount).ToList();
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
                    var favIcon = await FavIconService.GetFavIconImageSource(referringSiteModel.ReferrerUri.ToString()).ConfigureAwait(false);
                    return new MobileReferringSiteModel(referringSiteModel, favIcon);
                }

                return new MobileReferringSiteModel(referringSiteModel);
            }
        }
    }
}
