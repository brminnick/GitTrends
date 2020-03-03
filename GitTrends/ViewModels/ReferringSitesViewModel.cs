using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Xamarin.Forms;

namespace GitTrends
{
    class ReferringSitesViewModel : BaseViewModel
    {
        readonly GitHubApiV3Service _gitHubApiV3Service;

        bool _isRefreshing;
        bool _isActivityIndicatorVisible;

        public ReferringSitesViewModel(GitHubApiV3Service gitHubApiV3Service, AnalyticsService analyticsService) : base(analyticsService)
        {
            _gitHubApiV3Service = gitHubApiV3Service;
            RefreshCommand = new AsyncCommand<(string Owner, string Repository)>(repo => ExecuteRefreshCommand(repo.Owner, repo.Repository));

            //https://codetraveler.io/2019/09/11/using-observablecollection-in-a-multi-threaded-xamarin-forms-application/
            Xamarin.Forms.BindingBase.EnableCollectionSynchronization(ReferringSitesCollection, null, ObservableCollectionCallback);
        }

        public ObservableCollection<MobileReferringSiteModel> ReferringSitesCollection { get; } = new ObservableCollection<MobileReferringSiteModel>();

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

        async Task ExecuteRefreshCommand(string owner, string repository)
        {
            if (ReferringSitesCollection.Any())
            {
                ReferringSitesCollection.Clear();
            }
            else
            {
                //Only show the Activity Indicator when the page is first loaded
                IsActivityIndicatorVisible = true;
            }

            try
            {
                var referringSitesList = new List<MobileReferringSiteModel>();

                await foreach (var site in _gitHubApiV3Service.GetReferringSites(owner, repository).ConfigureAwait(false))
                {
                    referringSitesList.Add(site);
                }

                foreach (var site in referringSitesList.OrderByDescending(x => x.TotalCount))
                {
                    ReferringSitesCollection.Add(site);

                    if (Device.RuntimePlatform is Device.Android)
                    {
                        //Workaround for https://github.com/xamarin/Xamarin.Forms/issues/9753
                        await Task.Delay(500).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                IsActivityIndicatorVisible = IsRefreshing = false;
            }
        }

        //https://codetraveler.io/2019/09/11/using-observablecollection-in-a-multi-threaded-xamarin-forms-application/
        void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
        {
            lock (collection)
            {
                accessMethod?.Invoke();
            }
        }
    }
}
