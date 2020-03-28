using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class RepositoryPage : BasePage
    {
        readonly Query _searchBar, _settingsButton, _collectionView, _refreshView,
            _androidContextMenuOverflowButton, _androidSearchBarButton, _sortButton;

        public RepositoryPage(IApp app) : base(app, PageTitles.RepositoryPage)
        {
            _searchBar = GenerateMarkedQuery(RepositoryPageAutomationIds.SearchBar);
            _settingsButton = GenerateMarkedQuery(RepositoryPageAutomationIds.SettingsButton);
            _sortButton = GenerateMarkedQuery(RepositoryPageAutomationIds.SortButton);
            _collectionView = GenerateMarkedQuery(RepositoryPageAutomationIds.CollectionView);
            _refreshView = GenerateMarkedQuery(RepositoryPageAutomationIds.RefreshView);
            _androidContextMenuOverflowButton = x => x.Class("androidx.appcompat.widget.ActionMenuPresenter$OverflowMenuButton");
            _androidSearchBarButton = x => x.Id("ActionSearch");
        }

        public void TriggerPullToRefresh() => App.InvokeBackdoorMethod(BackdoorMethodConstants.TriggerPullToRefresh);

        public Task DismissSortingMenu()
        {
            if (App.Query(_androidContextMenuOverflowButton).Any())
            {
                App.Tap(_androidContextMenuOverflowButton);
                App.Screenshot("Tapped Android Search Bar Button");
            }

            App.Tap(_sortButton);
            App.Screenshot("Sort Button Tapped");

            App.Tap(PageTitle);

            App.Screenshot("Dismissed Sorting Options");

            return WaitForRepositoriesToFinishSorting();
        }

        public Task CancelSortingMenu()
        {
            if (App.Query(_androidContextMenuOverflowButton).Any())
            {
                App.Tap(_androidContextMenuOverflowButton);
                App.Screenshot("Tapped Android Search Bar Button");
            }

            App.Tap(_sortButton);
            App.Screenshot("Sort Button Tapped");

            App.Tap(SortingConstants.CancelText);
            App.Screenshot("Cancel Button Tapped");

            return WaitForRepositoriesToFinishSorting();
        }

        public Task SetSortingOption(SortingOption sortingOption)
        {
            if (App.Query(_androidContextMenuOverflowButton).Any())
            {
                App.Tap(_androidContextMenuOverflowButton);
                App.Screenshot("Tapped Android Search Bar Button");
            }

            App.Tap(_sortButton);
            App.Screenshot("Sort Button Tapped");

            var sortingOptionDescription = SortingConstants.SortingOptionsDictionary[sortingOption];
            var trendingOptionsRect = App.Query(sortingOptionDescription).Last().Rect;

            App.TapCoordinates(trendingOptionsRect.CenterX, trendingOptionsRect.CenterY);
            App.Screenshot($"{sortingOptionDescription} Tapped");

            return WaitForRepositoriesToFinishSorting();
        }

        public void TapRepository(string repositoryName)
        {
            App.ScrollDownTo(repositoryName);
            App.Tap(repositoryName);

            App.Screenshot($"Tapped {repositoryName}");
        }

        public void EnterSearchBarText(string text)
        {
            if (App.Query(_androidSearchBarButton).Any())
            {
                App.Tap(_androidSearchBarButton);
                App.Screenshot("Tapped Android Search Bar Button");
            }

            App.Tap(_searchBar);
            App.EnterText(text);
            App.DismissKeyboard();
            App.Screenshot($"Entered {text} into Search Bar");
        }

        public void TapSettingsButton()
        {
            if (App is AndroidApp)
            {
                App.Tap(_androidContextMenuOverflowButton);
                App.Screenshot("Android Overflow Button Tapped");
            }

            App.Tap(_settingsButton);
            App.Screenshot("Settings Button Tapped");
        }

        public void WaitForGitHubUserNotFoundPopup()
        {
            App.WaitForElement(GitHubUserNotFoundConstants.Title);
            App.Screenshot("GitHub User Not Found Popup Appeared");
        }

        public void DeclineGitHubUserNotFoundPopup()
        {
            App.Tap(GitHubUserNotFoundConstants.Decline);
            App.Screenshot("Declined GitHub User Not Found Popup");
        }

        public void AcceptGitHubUserNotFoundPopup()
        {
            App.Tap(GitHubUserNotFoundConstants.Accept);
            App.Screenshot("Accepted GitHub User Not Found Popup");
        }

        public List<Repository> GetVisibleRepositoryList()
        {
            var serializedRepositoryList = App.InvokeBackdoorMethod(BackdoorMethodConstants.GetVisibleCollection).ToString();
            return JsonConvert.DeserializeObject<List<Repository>>(serializedRepositoryList);
        }

        Task WaitForRepositoriesToFinishSorting() => Task.Delay(1000);
    }
}