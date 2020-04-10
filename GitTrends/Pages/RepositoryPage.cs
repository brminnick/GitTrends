using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Autofac;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class RepositoryPage : BaseContentPage<RepositoryViewModel>, ISearchPage
    {
        readonly WeakEventManager<string> _searchTextChangedEventManager = new WeakEventManager<string>();
        readonly RefreshView _refreshView;

        public RepositoryPage(RepositoryViewModel repositoryViewModel,
                                AnalyticsService analyticsService,
                                SortingService sortingService) : base(repositoryViewModel, analyticsService, PageTitles.RepositoryPage)
        {
            ViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;
            SearchBarTextChanged += HandleSearchBarTextChanged;

            var collectionView = new CollectionView
            {
                ItemTemplate = new RepositoryDataTemplateSelector(sortingService),
                BackgroundColor = Color.Transparent,
                SelectionMode = SelectionMode.Single,
                AutomationId = RepositoryPageAutomationIds.CollectionView,
                //Work around for https://github.com/xamarin/Xamarin.Forms/issues/9879
                Header = Device.RuntimePlatform is Device.Android ? new BoxView { HeightRequest = 8 } : null,
                Footer = Device.RuntimePlatform is Device.Android ? new BoxView { HeightRequest = 8 } : null
            };
            collectionView.SelectionChanged += HandleCollectionViewSelectionChanged;
            collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(RepositoryViewModel.VisibleRepositoryList));

            _refreshView = new RefreshView
            {
                AutomationId = RepositoryPageAutomationIds.RefreshView,
                Content = collectionView
            };
            _refreshView.SetDynamicResource(RefreshView.RefreshColorProperty, nameof(BaseTheme.PullToRefreshColor));
            _refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(RepositoryViewModel.IsRefreshing));
            _refreshView.SetBinding(RefreshView.CommandProperty, nameof(RepositoryViewModel.PullToRefreshCommand));

            var settingsToolbarItem = new ToolbarItem
            {
                Text = "Settings",
                Order = Device.RuntimePlatform is Device.Android ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Default,
                AutomationId = RepositoryPageAutomationIds.SettingsButton,
            };
            settingsToolbarItem.Clicked += HandleSettingsToolbarItemCliked;
            ToolbarItems.Add(settingsToolbarItem);

            var sortToolbarItem = new ToolbarItem
            {
                Text = "Sort",
                IconImageSource = Device.RuntimePlatform is Device.iOS ? "Sort" : null,
                Order = Device.RuntimePlatform is Device.Android ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Default,
                AutomationId = RepositoryPageAutomationIds.SortButton,
            };
            sortToolbarItem.Clicked += HandleSortToolbarItemCliked;
            ToolbarItems.Add(sortToolbarItem);

            //Work-around to prevent LargeNavigationBar from collapsing when CollectionView is scrolled; prevents janky animation when LargeNavigationBar collapses
            if (Device.RuntimePlatform is Device.iOS)
            {
                Content = new Grid
                {
                    Children =
                    {
                        new BoxView { HeightRequest = 0 },
                        _refreshView
                    }
                };
            }
            else
            {
                Content = _refreshView;
            }
        }

        public event EventHandler<string> SearchBarTextChanged
        {
            add => _searchTextChangedEventManager.AddEventHandler(value);
            remove => _searchTextChangedEventManager.RemoveEventHandler(value);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var token = await GitHubAuthenticationService.GetGitHubToken();

            if (!FirstRunService.IsFirstRun && shouldShowWelcomePage(Navigation, token.AccessToken))
            {
                using var scope = ContainerService.Container.BeginLifetimeScope();
                var welcomePage = scope.Resolve<WelcomePage>();

                //Allow RepositoryPage to appear briefly before loading 
                await Task.Delay(250);
                await Navigation.PushModalAsync(welcomePage);
            }
            else if (!FirstRunService.IsFirstRun
                        && isUserValid(token.AccessToken)
                        && _refreshView.Content is CollectionView collectionView
                        && IsNullOrEmpty(collectionView.ItemsSource))
            {
                _refreshView.IsRefreshing = true;
            }

            static bool shouldShowWelcomePage(in INavigation navigation, in string accessToken)
            {
                return !navigation.ModalStack.Any()
                        && GitHubAuthenticationService.Alias != DemoDataConstants.Alias
                        && !isUserValid(accessToken);
            }

            static bool isUserValid(in string accessToken) => !string.IsNullOrWhiteSpace(accessToken) || !string.IsNullOrWhiteSpace(GitHubAuthenticationService.Alias);

            static bool IsNullOrEmpty(in IEnumerable? enumerable) => !enumerable?.GetEnumerator().MoveNext() ?? true;
        }

        async void HandleCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;
            collectionView.SelectedItem = null;

            if (e?.CurrentSelection.FirstOrDefault() is Repository repository)
            {
                AnalyticsService.Track("Repository Tapped", new Dictionary<string, string>
                {
                    { nameof(Repository.OwnerLogin), repository.OwnerLogin },
                    { nameof(Repository.Name), repository.Name }
                });

                await NavigateToTrendsPage(repository);
            }
        }

        Task NavigateToSettingsPage()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();

            var profilePage = scope.Resolve<SettingsPage>();
            return MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(profilePage));
        }

        Task NavigateToTrendsPage(Repository repository)
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();

            var trendsPage = scope.Resolve<TrendsPage>(new TypedParameter(typeof(Repository), repository));
            return MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(trendsPage));
        }

        async void HandleSettingsToolbarItemCliked(object sender, EventArgs e)
        {
            AnalyticsService.Track("Settings Button Tapped");

            await NavigateToSettingsPage();
        }

        void HandlePullToRefreshFailed(object sender, PullToRefreshFailedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!Application.Current.MainPage.Navigation.ModalStack.Any()
                    && Application.Current.MainPage.Navigation.NavigationStack.Last() is RepositoryPage)
                {
                    await DisplayAlert(e.ErrorTitle, e.ErrorMessage, "OK");
                }
            });
        }

        async void HandleSortToolbarItemCliked(object sender, EventArgs e)
        {
            var sortingOptions = SortingConstants.SortingOptionsDictionary.Values;

            string? selection = await DisplayActionSheet("Sort By", SortingConstants.CancelText, null, sortingOptions.ToArray());

            if (!string.IsNullOrWhiteSpace(selection) && selection != SortingConstants.CancelText)
                ViewModel.SortRepositoriesCommand.Execute(SortingConstants.SortingOptionsDictionary.First(x => x.Value == selection).Key);
        }

        void HandleSearchBarTextChanged(object sender, string searchBarText) => ViewModel.FilterRepositoriesCommand.Execute(searchBarText);

        void ISearchPage.OnSearchBarTextChanged(in string text) => _searchTextChangedEventManager.HandleEvent(this, text, nameof(SearchBarTextChanged));
    }
}
