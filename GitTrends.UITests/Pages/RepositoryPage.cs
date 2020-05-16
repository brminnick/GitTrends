using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class RepositoryPage : BaseCollectionPage<Repository>
    {
        readonly Query _searchBar, _settingsButton, _collectionView, _refreshView,
            _androidContextMenuOverflowButton, _androidSearchBarButton, _sortButton, _emptyDataView,
            _smallScreenTrendingImage, _largeScreenTrendingImage;

        public RepositoryPage(IApp app) : base(app, PageTitles.RepositoryPage)
        {
            _searchBar = GenerateMarkedQuery(RepositoryPageAutomationIds.SearchBar);
            _settingsButton = GenerateMarkedQuery(RepositoryPageAutomationIds.SettingsButton);
            _sortButton = GenerateMarkedQuery(RepositoryPageAutomationIds.SortButton);
            _collectionView = GenerateMarkedQuery(RepositoryPageAutomationIds.CollectionView);
            _refreshView = GenerateMarkedQuery(RepositoryPageAutomationIds.RefreshView);
            _androidContextMenuOverflowButton = x => x.Class("androidx.appcompat.widget.ActionMenuPresenter$OverflowMenuButton");
            _androidSearchBarButton = x => x.Id("ActionSearch");
            _emptyDataView = GenerateMarkedQuery(RepositoryPageAutomationIds.EmptyDataView);
            _smallScreenTrendingImage = GenerateMarkedQuery(RepositoryPageAutomationIds.SmallScreenTrendingImage);
            _largeScreenTrendingImage = GenerateMarkedQuery(RepositoryPageAutomationIds.LargeScreenTrendingImage);
        }

        public bool IsEmptyDataViewVisible => App.Query(_emptyDataView).Any();

        public int SmallScreenTrendingImageCount => App.Query(_smallScreenTrendingImage).Count();
        public int LargeScreenTrendingImageCount => App.Query(_largeScreenTrendingImage).Count();

        public void WaitForEmptyDataView()
        {
            App.WaitForElement(_emptyDataView);
            App.Screenshot("Empty Data View Appeared");
        }

        public Task DismissSortingMenu()
        {
            if (App is AndroidApp && App.Query(_androidContextMenuOverflowButton).Any())
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
            if (App is AndroidApp && App.Query(_androidContextMenuOverflowButton).Any())
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
            if (App is AndroidApp && App.Query(_androidContextMenuOverflowButton).Any())
            {
                App.Tap(_androidContextMenuOverflowButton);
                App.Screenshot("Tapped Android Search Bar Button");
            }

            App.Tap(_sortButton);
            App.Screenshot("Sort Button Tapped");

            var sortingOptionDescription = SortingConstants.SortingOptionsDictionary[sortingOption];

            if (App is iOSApp)
            {
                var trendingOptionsRect = App.Query(sortingOptionDescription).Last().Rect;
                App.TapCoordinates(trendingOptionsRect.CenterX, trendingOptionsRect.CenterY);
            }
            else
            {
                App.Tap(sortingOptionDescription);
            }

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
            if (App is AndroidApp && App.Query(_androidSearchBarButton).Any())
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
            if (App is AndroidApp && App.Query(_androidContextMenuOverflowButton).Any())
            {
                App.Tap(_androidContextMenuOverflowButton);
                App.Screenshot("Android Overflow Button Tapped");
            }

            App.Tap(_settingsButton);
            App.Screenshot("Settings Button Tapped");
        }

        Task WaitForRepositoriesToFinishSorting() => Task.Delay(TimeSpan.FromSeconds(1));
    }
}