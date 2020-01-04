using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;

namespace GitTrends
{
    public class ReferringSitesViewModel : BaseViewModel
    {
        readonly GitHubApiV3Service _gitHubApiV3Service;

        bool _isRefreshing;

        public ReferringSitesViewModel(GitHubApiV3Service gitHubApiV3Service)
        {
            _gitHubApiV3Service = gitHubApiV3Service;
            RefreshCommand = new AsyncCommand<(string Owner, string Repository)>(repo => ExecuteRefreshCommand(repo.Owner, repo.Repository));

            //https://codetraveler.io/2019/09/11/using-observablecollection-in-a-multi-threaded-xamarin-forms-application/
            Xamarin.Forms.BindingBase.EnableCollectionSynchronization(ReferringSitesCollection, null, ObservableCollectionCallback);
        }

        public ObservableCollection<ReferingSiteModel> ReferringSitesCollection { get; } = new ObservableCollection<ReferingSiteModel>();

        public ICommand RefreshCommand { get; }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        async Task ExecuteRefreshCommand(string owner, string repository)
        {
            ReferringSitesCollection.Clear();

            try
            {
                var referringSites = await _gitHubApiV3Service.GetReferingSites(owner, repository);

                foreach (var site in referringSites)
                    ReferringSitesCollection.Add(site);
            }
            finally
            {
                IsRefreshing = false;
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
