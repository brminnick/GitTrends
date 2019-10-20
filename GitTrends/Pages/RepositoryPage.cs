using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public class RepositoryPage : BaseContentPage<RepositoryViewModel>, ISearchPage
    {
        readonly WeakEventManager<string> _searchTextChangedEventManager = new WeakEventManager<string>();
        readonly GitHubAuthenticationService _gitHubAuthenticationService;
        readonly RefreshView _repositoriesListRefreshView;

        bool _shouldNavigateToSettingsPageOnAppearing;

        public RepositoryPage(RepositoryViewModel repositoryViewModel, GitHubAuthenticationService gitHubAuthenticationService, bool isInitiatedByCallBackUri = false) : base(PageTitles.RepositoryPage, repositoryViewModel)
        {
            _shouldNavigateToSettingsPageOnAppearing = isInitiatedByCallBackUri;
            _gitHubAuthenticationService = gitHubAuthenticationService;

            ViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;
            SearchBarTextChanged += HandleSearchBarTextChanged;

            var collectionView = new CollectionView
            {
                ItemTemplate = new RepositoryDataTemplate(),
                BackgroundColor = Color.Transparent,
                SelectionMode = SelectionMode.Single
            };
            collectionView.SelectionChanged += HandleCollectionViewSelectionChanged;
            collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(RepositoryViewModel.VisibleRepositoryList));

            _repositoriesListRefreshView = new RefreshView
            {
                RefreshColor = ColorConstants.PullToRefreshActivityIndicatorColor,
                Content = collectionView
            };
            _repositoriesListRefreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(RepositoryViewModel.IsRefreshing));
            _repositoriesListRefreshView.SetBinding(RefreshView.CommandProperty, nameof(RepositoryViewModel.PullToRefreshCommand));

            var settingsToolbarItem = new ToolbarItem
            {
                Text = "Settings",
                Order = Device.RuntimePlatform is Device.Android ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Default
            };
            settingsToolbarItem.Clicked += HandleSettingsToolbarItem;
            ToolbarItems.Add(settingsToolbarItem);

            Content = _repositoriesListRefreshView;
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

            if (_shouldNavigateToSettingsPageOnAppearing)
            {
                _shouldNavigateToSettingsPageOnAppearing = false;
                await NavigateToSettingsPage();
            }
            else if (_repositoriesListRefreshView.Content is CollectionView collectionView
                        && collectionView.ItemsSource is ICollection<Repository> itemSource
                        && !itemSource.Any())
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
                    _repositoriesListRefreshView.IsRefreshing = true;
                }
            }
        }

        async void HandleCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;
            collectionView.SelectedItem = null;

            if (e?.CurrentSelection.FirstOrDefault() is Repository repository)
                await NavigateToTrendsPage(repository);
        }

        async Task NavigateToSettingsPage()
        {
            using (var scope = ContainerService.Container.BeginLifetimeScope())
            {
                var profilePage = scope.Resolve<SettingsPage>();
                await Device.InvokeOnMainThreadAsync(() => Navigation.PushAsync(profilePage));
            }
        }

        async Task NavigateToTrendsPage(Repository repository)
        {
            using (var scope = ContainerService.Container.BeginLifetimeScope())
            {
                var trendsPage = scope.Resolve<TrendsPage>(new TypedParameter(typeof(Repository), repository));
                await Device.InvokeOnMainThreadAsync(() => Navigation.PushAsync(trendsPage));
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

        void HandleSearchBarTextChanged(object sender, string searchBarText) => ViewModel.FilterRepositoriesCommand.Execute(searchBarText);
    }
}
