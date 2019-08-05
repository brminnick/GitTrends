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
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (ParentViewController.NavigationItem.SearchController is null)
            {
                var searchController = new UISearchController(searchResultsController: null)
                {
                    SearchResultsUpdater = this,
                    DimsBackgroundDuringPresentation = false,
                    HidesNavigationBarDuringPresentation = false,
                };
                searchController.SearchBar.Placeholder = string.Empty;

                ParentViewController.NavigationItem.SearchController = searchController;

                DefinesPresentationContext = true;
            }

            //Work-around to ensure the SearchController appears when the page opens https://stackoverflow.com/a/46313164/5953643
            ParentViewController.NavigationItem.SearchController.Active = true;
            ParentViewController.NavigationItem.SearchController.Active = false;
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            if (Element is ISearchPage searchPage)
                searchPage.OnSearchBarTextChanged(searchController.SearchBar.Text);
        }
    }
}
