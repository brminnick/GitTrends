using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public class RepositoryPage : BaseContentPage<RepositoryViewModel>, ISearchPage
    {
        readonly WeakEventManager<string> _searchTextChangedEventManager = new WeakEventManager<string>();

        readonly GitHubAuthenticationService _gitHubAuthenticationService;

        readonly ListView _listView;

        public RepositoryPage(RepositoryViewModel repositoryViewModel, GitHubAuthenticationService gitHubAuthenticationService) : base("Repositories", repositoryViewModel)
        {
            _gitHubAuthenticationService = gitHubAuthenticationService;

            ViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;
            SearchBarTextChanged += HandleSearchBarTextChanged;

            _listView = new ListView(ListViewCachingStrategy.RecycleElement)
            {
                IsPullToRefreshEnabled = true,
                ItemTemplate = new DataTemplate(typeof(RepositoryViewCell)),
                SeparatorVisibility = SeparatorVisibility.None,
                RowHeight = RepositoryViewCell.RowHeight,
                RefreshControlColor = ColorConstants.PullToRefreshActivityIndicatorColor,
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

                if (string.IsNullOrWhiteSpace(token.AccessToken) || string.IsNullOrWhiteSpace(_gitHubAuthenticationService.Alias))
                {
                    var shouldNavigateToSettingsPage = await DisplayAlert("GitHub User Not Found", "Sign in to GitHub on the Settings Page", "OK", "Cancel");

                    if (shouldNavigateToSettingsPage)
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
                    await NavigateToTrendsPage(repository);
                }
            }
        }

        Task NavigateToSettingsPage()
        {
            using (var scope = ContainerService.Container.BeginLifetimeScope())
            {
                var profilePage = scope.Resolve<ProfilePage>();
                return Device.InvokeOnMainThreadAsync(() => Navigation.PushAsync(profilePage));
            }
        }

        Task NavigateToTrendsPage(Repository repository)
        {
            using (var scope = ContainerService.Container.BeginLifetimeScope())
            {
                var trendsPage = scope.Resolve<TrendsPage>(new TypedParameter(typeof(Repository), repository));
                return Device.InvokeOnMainThreadAsync(() => Navigation.PushAsync(trendsPage));
            }
        }

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
