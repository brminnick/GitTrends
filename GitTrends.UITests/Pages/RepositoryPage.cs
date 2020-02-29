using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using Newtonsoft.Json;
using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class RepositoryPage : BasePage
    {
        readonly Query _searchBar, _settingsButton, _collectionView, _refreshView,
            _androidContextMenuOverflowButton, _androidSearchBarButton;

        public RepositoryPage(IApp app) : base(app, PageTitles.RepositoryPage)
        {
            _searchBar = GenerateMarkedQuery(RepositoryPageAutomationIds.SearchBar);
            _settingsButton = GenerateMarkedQuery(RepositoryPageAutomationIds.SettingsButton);
            _collectionView = GenerateMarkedQuery(RepositoryPageAutomationIds.CollectionView);
            _refreshView = GenerateMarkedQuery(RepositoryPageAutomationIds.RefreshView);
            _androidContextMenuOverflowButton = x => x.Class("androidx.appcompat.widget.ActionMenuPresenter$OverflowMenuButton");
            _androidSearchBarButton = x => x.Id("ActionSearch");
        }

        public async Task WaitForNoPullToRefresh(int timeoutInSeconds = 25)
        {
            int counter = 0;
            while (IsRefreshViewRefreshIndicatorDisplayed && counter < timeoutInSeconds)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                counter++;

                if (counter >= timeoutInSeconds)
                    throw new Exception($"Loading the list took longer than {timeoutInSeconds}");
            }
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
            if (App.Query(_androidContextMenuOverflowButton).Any())
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

        public void TriggerPullToRefresh() =>
            App.InvokeBackdoorMethod(BackdoorMethodConstants.TriggerPullToRefresh);

        public List<Repository> GetVisibleRepositoryList()
        {
            var serializedRepositoryList = App.InvokeBackdoorMethod(BackdoorMethodConstants.GetVisibleRepositoryList).ToString();
            return JsonConvert.DeserializeObject<List<Repository>>(serializedRepositoryList);
        }
    }
}