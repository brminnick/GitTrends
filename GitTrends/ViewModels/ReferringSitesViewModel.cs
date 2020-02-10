using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;
using HtmlAgilityPack;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

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

        public ObservableCollection<MobileReferringSiteModel> ReferringSitesCollection { get; } = new ObservableCollection<MobileReferringSiteModel>();

        public ICommand RefreshCommand { get; }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        static void InsertInPlace<TItem, TKey>(ObservableCollection<TItem> collection, TItem itemToAdd, Func<TItem, TKey> keyGetter)
        {
            int index = BinarySearch(collection.ToList(), keyGetter(itemToAdd), Comparer<TKey>.Default, keyGetter);
            collection.Insert(index, itemToAdd);

            static int BinarySearch(IList<TItem> collection, TKey keyToFind, IComparer<TKey> comparer, Func<TItem, TKey> keyGetter)
            {
                int lower = 0;
                int upper = collection.Count - 1;

                while (lower <= upper)
                {
                    int middle = lower + (upper - lower) / 2;
                    int comparisonResult = comparer.Compare(keyGetter.Invoke(collection[middle]), keyToFind);
                    if (comparisonResult == 0)
                    {
                        return middle;
                    }
                    else if (comparisonResult < 0)
                    {
                        upper = middle - 1;
                    }
                    else
                    {
                        lower = middle + 1;
                    }
                }

                return lower;
            }
        }

        async Task ExecuteRefreshCommand(string owner, string repository)
        {
            ReferringSitesCollection.Clear();

            try
            {
                await foreach (var site in _gitHubApiV3Service.GetReferingSites(owner, repository).ConfigureAwait(false))
                {
                    InsertInPlace(ReferringSitesCollection, site, x => x.TotalCount);

                    //Allow CollectionView to Animate on iOS
                    if (Device.RuntimePlatform is Device.iOS)
                        await Task.Delay(250);
                }
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
