using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using Autofac;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
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
        readonly FirstRunService _firstRunService;
        readonly GitHubUserService _gitHubUserService;
        readonly DeepLinkingService _deepLinkingService;

        public RepositoryPage(IMainThread mainThread,
                                FirstRunService firstRunService,
                                IAnalyticsService analyticsService,
                                GitHubUserService gitHubUserService,
                                MobileSortingService sortingService,
                                DeepLinkingService deepLinkingService,
                                RepositoryViewModel repositoryViewModel) : base(repositoryViewModel, analyticsService, mainThread)
        {
            _firstRunService = firstRunService;
            _gitHubUserService = gitHubUserService;
            _deepLinkingService = deepLinkingService;

            SearchBarTextChanged += HandleSearchBarTextChanged;
            RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;
            LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

            _refreshView = new RefreshView
            {
                AutomationId = RepositoryPageAutomationIds.RefreshView,
                Content = new CollectionView
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
                            .Bind(EmptyDataView.TitleProperty, nameof(RepositoryViewModel.EmptyDataViewTitle))
                            .Bind(EmptyDataView.DescriptionProperty, nameof(RepositoryViewModel.EmptyDataViewDescription))

                }.Bind(CollectionView.ItemsSourceProperty, nameof(RepositoryViewModel.VisibleRepositoryList))
                 .Bind<CollectionView, IReadOnlyList<Repository>, Label?>(CollectionView.HeaderProperty, nameof(RepositoryViewModel.VisibleRepositoryList))
                 .Invoke(collectionView => collectionView.SelectionChanged += HandleCollectionViewSelectionChanged)
                 .Invoke(collectionView => collectionView.SetBinding(CollectionView.HeaderProperty, new MultiBinding
                 {
                     Converter = new CollectionViewHeaderMultiValueConverter(),
                     Bindings =
                     {
                         new Binding(nameof(RepositoryViewModel.IsRefreshing)),
                         new Binding(nameof(repositoryViewModel.VisibleRepositoryList)),
                         new Binding(nameof(CollectionView.Header), source: collectionView)
                     }
                 }))
            }.DynamicResource(RefreshView.RefreshColorProperty, nameof(BaseTheme.PullToRefreshColor))
             .Bind(RefreshView.IsRefreshingProperty, nameof(RepositoryViewModel.IsRefreshing))
             .Bind(RefreshView.CommandProperty, nameof(RepositoryViewModel.PullToRefreshCommand));

            ToolbarItems.Add(new ToolbarItem
            {
                Text = PageTitles.SettingsPage,
                IconImageSource = Device.RuntimePlatform is Device.iOS ? "Settings" : null,
                Order = Device.RuntimePlatform is Device.Android ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Default,
                AutomationId = RepositoryPageAutomationIds.SettingsButton,
                Command = new AsyncCommand(ExecuteSetttingsToolbarItemCommand)
            });

            ToolbarItems.Add(new ToolbarItem
            {
                Text = RepositoryPageConstants.SortToolbarItemText,
                Priority = 1,
                IconImageSource = Device.RuntimePlatform is Device.iOS ? "Sort" : null,
                Order = Device.RuntimePlatform is Device.Android ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Default,
                AutomationId = RepositoryPageAutomationIds.SortButton,
                Command = new AsyncCommand(ExecuteSortToolbarItemCommand)
            });

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

            this.SetBinding(TitleProperty, nameof(RepositoryViewModel.TitleText));
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
                var welcomePage = ContainerService.Container.Resolve<WelcomePage>();

                //Allow RepositoryPage to appear briefly before loading 
                await Task.Delay(TimeSpan.FromMilliseconds(250));
                await Navigation.PushModalAsync(welcomePage);
            }
            else if (!_firstRunService.IsFirstRun
                        && isUserValid(token.AccessToken, _gitHubUserService.Alias)
                        && _refreshView.Content is CollectionView collectionView
                        && collectionView.ItemsSource.IsNullOrEmpty())
            {
                _refreshView.IsRefreshing = true;
            }

            bool shouldShowWelcomePage(in INavigation navigation, in string accessToken)
            {
                return !navigation.ModalStack.Any()
                        && _gitHubUserService.Alias != DemoUserConstants.Alias
                        && !isUserValid(accessToken, _gitHubUserService.Alias);
            }

            static bool isUserValid(in string accessToken, in string alias) => !string.IsNullOrWhiteSpace(accessToken) || !string.IsNullOrWhiteSpace(alias);
        }

        async void HandleCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;
            collectionView.SelectedItem = null;

            if (e?.CurrentSelection.FirstOrDefault() is Repository repository)
            {
                AnalyticsService.Track("Repository Tapped", new Dictionary<string, string>
                {
                    { nameof(Repository) + nameof(Repository.OwnerLogin), repository.OwnerLogin },
                    { nameof(Repository) + nameof(Repository.Name), repository.Name }
                });

                await NavigateToTrendsPage(repository);
            }
        }

        Task NavigateToSettingsPage()
        {
            var settingsPage = ContainerService.Container.Resolve<SettingsPage>();
            return MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(settingsPage));
        }

        Task NavigateToTrendsPage(Repository repository)
        {
            var trendsPage = ContainerService.Container.Resolve<TrendsPage>(new TypedParameter(typeof(Repository), repository));
            return MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(trendsPage));
        }

        Task ExecuteSetttingsToolbarItemCommand()
        {
            AnalyticsService.Track("Settings Button Tapped");

            return NavigateToSettingsPage();
        }

        async Task ExecuteSortToolbarItemCommand()
        {
            var sortingOptions = MobileSortingService.SortingOptionsDictionary.Values;

            string? selection = await DisplayActionSheet(SortingConstants.ActionSheetTitle, SortingConstants.CancelText, null, sortingOptions.ToArray());

            if (!string.IsNullOrWhiteSpace(selection) && selection != SortingConstants.CancelText)
                ViewModel.SortRepositoriesCommand.Execute(MobileSortingService.SortingOptionsDictionary.First(x => x.Value == selection).Key);
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

        void HandlePreferredLanguageChanged(object sender, string? e)
        {
            var sortItem = ToolbarItems.First(x => x.AutomationId is RepositoryPageAutomationIds.SortButton);
            var settingsItem = ToolbarItems.First(x => x.AutomationId is RepositoryPageAutomationIds.SettingsButton);

            sortItem.Text = RepositoryPageConstants.SortToolbarItemText;
            settingsItem.Text = PageTitles.SettingsPage;
        }

        void HandleSearchBarTextChanged(object sender, string searchBarText) => ViewModel.FilterRepositoriesCommand.Execute(searchBarText);

        void ISearchPage.OnSearchBarTextChanged(in string text) => _searchTextChangedEventManager.RaiseEvent(this, text, nameof(SearchBarTextChanged));

        class CollectionViewHeaderMultiValueConverter : IMultiValueConverter
        {
            public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                if (values[0] is bool isRefreshing
                    && values[1] is IList<Repository> repositoryList)
                {
                    var header = (View?)values[2];

                    if ((!isRefreshing || header != null) && repositoryList.Any(x => x.DailyViewsList.Any()))
                    {
                        var text = $"Impact: Views {repositoryList.Sum(x => x.TotalViews)}";
                        return new StatisticsLabel(text, true, nameof(BaseTheme.PrimaryTextColor)).FillExpandHorizontal().TextCenter();
                    }

                    return Device.RuntimePlatform is Device.Android ? new BoxView { HeightRequest = BaseRepositoryDataTemplate.BottomPadding } : header;
                }

                return null;
            }

            public object[] ConvertBack(object? value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }
}
