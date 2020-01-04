using System;
using System.Collections;
using System.Linq;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public class ReferringSitesPage : BaseContentPage<ReferringSitesViewModel>
    {
        public ReferringSitesPage(ReferringSitesViewModel refferringSitesViewModel, Repository repository) : base("Referring Sites", refferringSitesViewModel)
        {
            var collectionView = new CollectionView
            {
                ItemTemplate = new ReferringSitesDataTemplate(),
                SelectionMode = SelectionMode.Single
            };
            collectionView.SelectionChanged += HandleCollectionViewSelectionChanged;
            collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(ReferringSitesViewModel.ReferringSitesCollection));

            var refreshView = new RefreshView
            {
                CommandParameter = (repository.OwnerLogin, repository.Name),
                Content = collectionView
            };
            refreshView.SetDynamicResource(RefreshView.RefreshColorProperty, nameof(BaseTheme.RefreshControlColor));
            refreshView.SetBinding(RefreshView.CommandProperty, nameof(ReferringSitesViewModel.RefreshCommand));
            refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(ReferringSitesViewModel.IsRefreshing));

            Content = refreshView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Disappearing += HandleDisappearing;

            if (Content is RefreshView refreshView
                        && refreshView.Content is CollectionView collectionView
                        && IsNullOrEmpty(collectionView.ItemsSource))
            {
                refreshView.IsRefreshing = true;
                refreshView.IsEnabled = false;
            }

            static bool IsNullOrEmpty(in IEnumerable? enumerable) => !enumerable?.GetEnumerator().MoveNext() ?? true;
        }

        async void HandleCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;
            collectionView.SelectedItem = null;

            if (e?.CurrentSelection.FirstOrDefault() is ReferringSiteModel referingSite
                && referingSite.IsReferrerUriValid
                && referingSite.ReferrerUri != null)
            {
                Disappearing -= HandleDisappearing;

                await OpenBrowser(referingSite.ReferrerUri);
            }
        }

        async void HandleDisappearing(object sender, EventArgs e)
        {
            if (Navigation.ModalStack.Any())
                await Navigation.PopModalAsync();
        }
    }
}
