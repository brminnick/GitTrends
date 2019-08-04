using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public class RepositoryPage : BaseContentPage<RepositoryViewModel>
    {
        readonly ListView _listView;

        public RepositoryPage() : base("Repositories")
        {
            ViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

            _listView = new ListView(ListViewCachingStrategy.RecycleElement)
            {
                IsPullToRefreshEnabled = true,
                ItemTemplate = new DataTemplate(typeof(RepositoryViewCell)),
                SeparatorVisibility = SeparatorVisibility.None,
                RowHeight = RepositoryViewCell.RowHeight,
                RefreshControlColor = ColorConstants.ActivityIndicatorColor,
                BackgroundColor = Color.Transparent,
                SelectionMode = ListViewSelectionMode.None
            };
            _listView.ItemTapped += HandleListViewItemTapped;
            _listView.SetBinding(ListView.IsRefreshingProperty, nameof(RepositoryViewModel.IsRefreshing));
            _listView.SetBinding(ListView.ItemsSourceProperty, nameof(RepositoryViewModel.RepositoryCollection));
            _listView.SetBinding(ListView.RefreshCommandProperty, nameof(RepositoryViewModel.PullToRefreshCommand));

            var settingsToolbarItem = new ToolbarItem { Text = "Settings" };
            settingsToolbarItem.Clicked += HandleSettingsToolbarItem;
            ToolbarItems.Add(settingsToolbarItem);

            BackgroundColor = ColorConstants.LightBlue;

            Content = _listView;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel.RepositoryCollection?.Any() != true)
            {
                var token = await GitHubAuthenticationService.GetGitHubToken();

                if (token?.AccessToken != null && !string.IsNullOrWhiteSpace(GitHubAuthenticationService.Alias))
                {
                    _listView.BeginRefresh();
                }
                else
                {
                    await NavigateToSettingsPage();
                }
            }
        }

        async void HandleListViewItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (sender is ListView listView)
            {
                listView.SelectedItem = null;

                if (!listView.IsRefreshing && e.Item is Repository repository)
                {
                    await Navigation.PushAsync(new TrendsPage(repository.OwnerLogin, repository.Name));
                }
            }
        }

        Task NavigateToSettingsPage() => Device.InvokeOnMainThreadAsync(() => Navigation.PushAsync(new ProfilePage()));

        async void HandleSettingsToolbarItem(object sender, EventArgs e) => await NavigateToSettingsPage();

        void HandlePullToRefreshFailed(object sender, PullToRefreshFailedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (!Application.Current.MainPage.Navigation.ModalStack.Any()
                    && Application.Current.MainPage.Navigation.NavigationStack.Last() is RepositoryPage)
                {
                    await DisplayAlert(e.ErrorTitle, e.ErrorMessage, "OK");
                }
            });
        }
    }
}
