using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GitTrends;
using GitTrends.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RepositoryPage), typeof(SearchPageRenderer))]
namespace GitTrends.iOS
{
    public class SearchPageRenderer : PageRenderer, IUISearchResultsUpdating
    {
        bool _isFirstAppearing = true;

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            base.WillMoveToParentViewController(parent);

            var searchController = new UISearchController(searchResultsController: null)
            {
                SearchResultsUpdater = this,
                DimsBackgroundDuringPresentation = false,
                HidesNavigationBarDuringPresentation = false,
                HidesBottomBarWhenPushed = true
            };
            searchController.SearchBar.Placeholder = string.Empty;

            parent.NavigationItem.SearchController = searchController;

            DefinesPresentationContext = true;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            //Work-around to ensure the SearchController appears when the page first appears https://stackoverflow.com/a/46313164/5953643
            if (_isFirstAppearing)
            {
                ParentViewController.NavigationItem.SearchController.Active = true;
                ParentViewController.NavigationItem.SearchController.Active = false;

                _isFirstAppearing = false;
            }
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            if (Element is ISearchPage searchPage)
                searchPage.OnSearchBarTextChanged(searchController.SearchBar.Text);
        }
    }
}
