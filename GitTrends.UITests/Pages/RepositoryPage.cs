using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
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
            _smallScreenTrendingImage, _largeScreenTrendingImage, _informationButton,
            _statistic1FloatingActionButton, _statistic2FloatingActionButton, _statistic3FloatingActionButton;

        public RepositoryPage(IApp app) : base(app, () => PageTitles.RepositoryPage)
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
            _informationButton = GenerateMarkedQuery(RepositoryPageAutomationIds.InformationButton);
            _statistic1FloatingActionButton = GenerateMarkedQuery(RepositoryPageAutomationIds.GetFloatingActionTextButtonLabelAutomationId(FloatingActionButtonType.Statistic1));
            _statistic2FloatingActionButton = GenerateMarkedQuery(RepositoryPageAutomationIds.GetFloatingActionTextButtonLabelAutomationId(FloatingActionButtonType.Statistic2));
            _statistic3FloatingActionButton = GenerateMarkedQuery(RepositoryPageAutomationIds.GetFloatingActionTextButtonLabelAutomationId(FloatingActionButtonType.Statistic3));
        }

        public bool IsEmptyDataViewVisible => App.Query(_emptyDataView).Any();

        public int SmallScreenTrendingImageCount => App.Query(_smallScreenTrendingImage).Count();
        public int LargeScreenTrendingImageCount => App.Query(_largeScreenTrendingImage).Count();

        public string InformationButtonStatistic1Text => GetText(_statistic1FloatingActionButton);
        public string InformationButtonStatistic2Text => GetText(_statistic2FloatingActionButton);
        public string InformationButtonStatistic3Text => GetText(_statistic3FloatingActionButton);

        public void TapInformationButton()
        {
            App.Tap(_informationButton);
            App.Screenshot("Information Button Tapped");
        }

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

            App.WaitForElement(SortingConstants.ActionSheetTitle);

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

            App.WaitForElement(SortingConstants.ActionSheetTitle);

            //iPads use a UIPopoverView which does not provide a 'Cancel' option in the
            if (!App.Query(SortingConstants.CancelText).Any())
                App.Tap(PageTitle);
            else
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

            var sortingOptionDescription = MobileSortingService.SortingOptionsDictionary[sortingOption];

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

        public string GetSettingsButtonText()
        {
            if (App is iOSApp)
                throw new NotSupportedException();

            App.Tap(_androidContextMenuOverflowButton);
            App.Screenshot("Tapped Android Search Bar Button");

            var settingsButtonText = GetText(_settingsButton);

            App.Tap(PageTitle);
            App.Screenshot("Dismissed Android Search Bar Button");

            return settingsButtonText;
        }

        public string GetSortButtonText()
        {
            if (App is iOSApp)
                throw new NotSupportedException();

            App.Tap(_androidContextMenuOverflowButton);
            App.Screenshot("Tapped Android Search Bar Button");

            var sortButtonText = GetText(_sortButton);

            App.Tap(PageTitle);
            App.Screenshot("Dismissed Android Search Bar Button");

            return sortButtonText;
        }

        Task WaitForRepositoriesToFinishSorting() => Task.Delay(TimeSpan.FromSeconds(1));
    }
}