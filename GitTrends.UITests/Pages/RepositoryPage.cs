using Xamarin.UITest;
using GitTrends.Mobile.Shared;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class RepositoryPage : BasePage
    {
        readonly Query _searchBar, _settingsButton, _collectionView, _refreshView;

        public RepositoryPage(IApp app) : base(app, PageTitles.RepositoryPage)
        {
            _searchBar = GenerateQuery(RepositoryPageAutomationIds.SearchBar);
            _settingsButton = GenerateQuery(RepositoryPageAutomationIds.SettingsButton);
            _collectionView = GenerateQuery(RepositoryPageAutomationIds.CollectionView);
            _refreshView = GenerateQuery(RepositoryPageAutomationIds.RefreshView);
        }

        public void EnterSearchBarText(string text) => EnterText(_searchBar, text);

        public void TapSettingsButton() => App.Tap(_settingsButton);

        public void WaitForGitHubUserNotFoundPopup() => App.WaitForElement(GitHubUserNotFoundConstants.Title);
        public void DeclineGitHubUserNotFoundPopup() => App.Tap(GitHubUserNotFoundConstants.Decline);
        public void AcceptGitHubUserNotFoundPopup() => App.Tap(GitHubUserNotFoundConstants.Accept);
    }
}