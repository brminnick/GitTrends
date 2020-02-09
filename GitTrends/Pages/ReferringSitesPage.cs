using System;
using System.Collections;
using System.Linq;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public class ReferringSitesPage : BaseContentPage<ReferringSitesViewModel>
    {
        const string _title = "Referring Sites";

        readonly RefreshView _refreshView;

        public ReferringSitesPage(ReferringSitesViewModel referringSitesViewModel, Repository repository) : base(_title, referringSitesViewModel)
        {
            var collectionView = new CollectionView
            {
                ItemTemplate = new ReferringSitesDataTemplateSelector(),
                SelectionMode = SelectionMode.Single
            };
            collectionView.SelectionChanged += HandleCollectionViewSelectionChanged;
            collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(ReferringSitesViewModel.ReferringSitesCollection));

            _refreshView = new RefreshView
            {
                InputTransparent = true,
                CommandParameter = (repository.OwnerLogin, repository.Name),
                Content = collectionView
            };
            _refreshView.SetDynamicResource(RefreshView.RefreshColorProperty, nameof(BaseTheme.RefreshControlColor));
            _refreshView.SetBinding(RefreshView.CommandProperty, nameof(ReferringSitesViewModel.RefreshCommand));
            _refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(ReferringSitesViewModel.IsRefreshing));

            if (Device.RuntimePlatform is Device.iOS)
            {
                var titleLabel = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    Text = _title,
                    FontSize = 30,
                    VerticalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(10, 0)
                };
                titleLabel.SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.TextColor));

                var grid = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition { Height = new GridLength(50, GridUnitType.Absolute) },
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    },
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    }
                };

                grid.Children.Add(titleLabel, 0, 0);
                grid.Children.Add(_refreshView, 0, 1);

                Content = grid;
            }
            else
            {
                Content = _refreshView;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Disappearing += HandleDisappearing;

            if (_refreshView.Content is CollectionView collectionView && IsNullOrEmpty(collectionView.ItemsSource))
                _refreshView.IsRefreshing = true;

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
