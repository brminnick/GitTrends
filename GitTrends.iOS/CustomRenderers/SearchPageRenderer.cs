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
        RepositoryPage? _repositoryPage;

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            var searchController = new UISearchController(searchResultsController: null)
            {
                SearchResultsUpdater = this,
                DimsBackgroundDuringPresentation = false,
                HidesNavigationBarDuringPresentation = false
            };
            searchController.SearchBar.Placeholder = "Filter";
            searchController.SearchBar.TintColor = Color.White.ToUIColor();

            parent.NavigationItem.SearchController = searchController;

            DefinesPresentationContext = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (Element is RepositoryPage repositoryPage)
                _repositoryPage = repositoryPage;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            ////Work-around to ensure the filter appears when the page opens https://stackoverflow.com/a/46313164/5953643

            ParentViewController.NavigationItem.SearchController.Active = true;
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController) => _repositoryPage?.OnSearchBarTextChanged(searchController.SearchBar.Text);
    }
}
