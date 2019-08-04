using System;
using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public class RepositoryPage : BaseContentPage<RepositoryViewModel>
    {
        readonly WeakEventManager<string> _searchTextChangedEventManager = new WeakEventManager<string>();

        readonly ListView _listView;

        public RepositoryPage() : base("Repositories")
        {
            ViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;
            SearchBarTextChanged += HandleSearchBarTextChanged;

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
            _listView.SetBinding(ListView.ItemsSourceProperty, nameof(RepositoryViewModel.VisibleRepositoryCollection));
            _listView.SetBinding(ListView.RefreshCommandProperty, nameof(RepositoryViewModel.PullToRefreshCommand));

            var settingsToolbarItem = new ToolbarItem
            {
                Text = "Settings",
                Order = Device.RuntimePlatform is Device.Android ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Default
            };
            settingsToolbarItem.Clicked += HandleSettingsToolbarItem;
            ToolbarItems.Add(settingsToolbarItem);

            Content = _listView;
        }

        public event EventHandler<string> SearchBarTextChanged
        {
            add => _searchTextChangedEventManager.AddEventHandler(value);
            remove => _searchTextChangedEventManager.RemoveEventHandler(value);
        }

        public void OnSearchBarTextChanged(in string text) => _searchTextChangedEventManager.HandleEvent(this, text, nameof(SearchBarTextChanged));

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel.VisibleRepositoryCollection?.Any() != true)
            {
                var token = await GitHubAuthenticationService.GetGitHubToken();

                if (string.IsNullOrWhiteSpace(token.AccessToken) || string.IsNullOrWhiteSpace(GitHubAuthenticationService.Alias))
                {
                    await NavigateToSettingsPage();
                }
                else
                {
                    _listView.BeginRefresh();
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

        void HandleSearchBarTextChanged(object sender, string searchBarText) => ViewModel.FilterRepositoriesCommand?.Execute(searchBarText);
    }
}
