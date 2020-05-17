using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using Autofac;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    public class RepositoryPage : BaseContentPage<RepositoryViewModel>, ISearchPage
    {
        readonly WeakEventManager<string> _searchTextChangedEventManager = new WeakEventManager<string>();
        readonly RefreshView _refreshView;
        readonly DeepLinkingService _deepLinkingService;
        readonly FirstRunService _firstRunService;
        readonly GitHubUserService _gitHubUserService;

        public RepositoryPage(RepositoryViewModel repositoryViewModel,
                                IAnalyticsService analyticsService,
                                SortingService sortingService,
                                DeepLinkingService deepLinkingService,
                                IMainThread mainThread,
                                FirstRunService firstRunService,
                                GitHubUserService gitHubUserService) : base(repositoryViewModel, analyticsService, mainThread, PageTitles.RepositoryPage)
        {
            _firstRunService = firstRunService;
            _gitHubUserService = gitHubUserService;
            _deepLinkingService = deepLinkingService;

            ViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;
            SearchBarTextChanged += HandleSearchBarTextChanged;

            var collectionView = new CollectionView
            {
                ItemTemplate = new RepositoryDataTemplateSelector(sortingService),
                BackgroundColor = Color.Transparent,
                SelectionMode = SelectionMode.Single,
                AutomationId = RepositoryPageAutomationIds.CollectionView,
                //Work around for https://github.com/xamarin/Xamarin.Forms/issues/9879
                Header = Device.RuntimePlatform is Device.Android ? new BoxView { HeightRequest = BaseRepositoryDataTemplate.BottomPadding } : null,
                Footer = Device.RuntimePlatform is Device.Android ? new BoxView { HeightRequest = BaseRepositoryDataTemplate.TopPadding } : null,
                EmptyView = new EmptyDataView("EmptyRepositoriesList", RepositoryPageAutomationIds.EmptyDataView)
                            .Bind<EmptyDataView, bool, bool>(IsVisibleProperty, nameof(RepositoryViewModel.IsRefreshing), convert: isRefreshing => !isRefreshing)
                            .Bind(EmptyDataView.TextProperty, nameof(RepositoryViewModel.EmptyDataViewText))

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
                IconImageSource = Device.RuntimePlatform is Device.iOS ? "Settings" : null,
                Order = Device.RuntimePlatform is Device.Android ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Default,
                AutomationId = RepositoryPageAutomationIds.SettingsButton,
                Command = new AsyncCommand(ExecuteSetttingsToolbarItemCommand)
            };
            ToolbarItems.Add(settingsToolbarItem);

            var sortToolbarItem = new ToolbarItem
            {
                Text = "Sort",
                Priority = 1,
                IconImageSource = Device.RuntimePlatform is Device.iOS ? "Sort" : null,
                Order = Device.RuntimePlatform is Device.Android ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Default,
                AutomationId = RepositoryPageAutomationIds.SortButton,
                Command = new AsyncCommand(ExecuteSortToolbarItemCommand)
            };
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

            var token = await _gitHubUserService.GetGitHubToken();

            if (!_firstRunService.IsFirstRun && shouldShowWelcomePage(Navigation, token.AccessToken))
            {
                using var scope = ContainerService.Container.BeginLifetimeScope();
                var welcomePage = scope.Resolve<WelcomePage>();

                //Allow RepositoryPage to appear briefly before loading 
                await Task.Delay(TimeSpan.FromMilliseconds(250));
                await Navigation.PushModalAsync(welcomePage);
            }
            else if (!_firstRunService.IsFirstRun
                        && isUserValid(token.AccessToken)
                        && _refreshView.Content is CollectionView collectionView
                        && IsNullOrEmpty(collectionView.ItemsSource))
            {
                _refreshView.IsRefreshing = true;
            }

            bool shouldShowWelcomePage(in INavigation navigation, in string accessToken)
            {
                return !navigation.ModalStack.Any()
                        && _gitHubUserService.Alias != DemoDataConstants.Alias
                        && !isUserValid(accessToken);
            }

            bool isUserValid(in string accessToken) => !string.IsNullOrWhiteSpace(accessToken) || !string.IsNullOrWhiteSpace(_gitHubUserService.Alias);

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

        Task ExecuteSetttingsToolbarItemCommand()
        {
            AnalyticsService.Track("Settings Button Tapped");

            return NavigateToSettingsPage();
        }

        async Task ExecuteSortToolbarItemCommand()
        {
            var sortingOptions = SortingConstants.SortingOptionsDictionary.Values;

            string? selection = await DisplayActionSheet("Sort By", SortingConstants.CancelText, null, sortingOptions.ToArray());

            if (!string.IsNullOrWhiteSpace(selection) && selection != SortingConstants.CancelText)
                ViewModel.SortRepositoriesCommand.Execute(SortingConstants.SortingOptionsDictionary.First(x => x.Value == selection).Key);
        }

        void HandlePullToRefreshFailed(object sender, PullToRefreshFailedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!Application.Current.MainPage.Navigation.ModalStack.Any()
                    && Application.Current.MainPage.Navigation.NavigationStack.Last() is RepositoryPage)
                {
                    if (e.Accept is null)
                    {
                        await DisplayAlert(e.Title, e.Message, e.Cancel);
                    }
                    else
                    {
                        var isAccepted = await DisplayAlert(e.Title, e.Message, e.Accept, e.Cancel);
                        if (isAccepted)
                            await _deepLinkingService.OpenBrowser(GitHubConstants.GitHubRateLimitingDocs);
                    }
                }
            });
        }

        void HandleSearchBarTextChanged(object sender, string searchBarText) => ViewModel.FilterRepositoriesCommand.Execute(searchBarText);

        void ISearchPage.OnSearchBarTextChanged(in string text) => _searchTextChangedEventManager.HandleEvent(this, text, nameof(SearchBarTextChanged));
    }
}
