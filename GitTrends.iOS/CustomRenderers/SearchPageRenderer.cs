using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        readonly UISearchController _searchController;

        public SearchPageRenderer()
        {
            _searchController = new UISearchController(searchResultsController: null)
            {
                SearchResultsUpdater = this,
                DimsBackgroundDuringPresentation = false,
                HidesNavigationBarDuringPresentation = false,
                HidesBottomBarWhenPushed = true
            };
            _searchController.SearchBar.Placeholder = string.Empty;
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (ParentViewController is UIViewController parentViewController)
            {
                if (parentViewController.NavigationItem.SearchController is null)
                {
                    ParentViewController.NavigationItem.SearchController = _searchController;
                    DefinesPresentationContext = true;

                    //Work-around to ensure the SearchController appears when the page first appears https://stackoverflow.com/a/46313164/5953643
                    ParentViewController.NavigationItem.SearchController.Active = true;
                    ParentViewController.NavigationItem.SearchController.Active = false;
                }

                await UpdateBarButtonItems(parentViewController, (Page)Element);
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (ParentViewController != null)
                ParentViewController.NavigationItem.SearchController = null;
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            if (Element is ISearchPage searchPage)
                searchPage.OnSearchBarTextChanged(searchController.SearchBar.Text ?? string.Empty);
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.NewElement is not null)
                e.NewElement.SizeChanged += HandleSizeChanged;
        }

        async Task UpdateBarButtonItems(UIViewController parentViewController, Page page)
        {
            var (leftBarButtonItems, rightBarButtonItems) = await GetToolbarItems(page.ToolbarItems);

            parentViewController.NavigationItem.LeftBarButtonItems = leftBarButtonItems.Any() switch
            {
                true => [.. leftBarButtonItems],
                false => []
            };

            parentViewController.NavigationItem.RightBarButtonItems = rightBarButtonItems.Any() switch
            {
                true => [.. rightBarButtonItems],
                false => []
            };
        }

        async Task<(IReadOnlyList<UIBarButtonItem>? LeftBarButtonItem, IReadOnlyList<UIBarButtonItem> RightBarButtonItems)> GetToolbarItems(IEnumerable<ToolbarItem> items)
        {
            var leftBarButtonItems = new List<UIBarButtonItem>();
            foreach (var item in items.Where(static x => x.Priority is 1))
            {
                var barButtonItem = await GetUIBarButtonItem(item);
                leftBarButtonItems.Add(barButtonItem);
            }

            var rightBarButtonItems = new List<UIBarButtonItem>();
            foreach (var item in items.Where(static x => x.Priority != 1))
            {
                var barButtonItem = await GetUIBarButtonItem(item);
                rightBarButtonItems.Add(barButtonItem);
            }

            return (leftBarButtonItems, rightBarButtonItems);
        }

        static async Task<UIBarButtonItem> GetUIBarButtonItem(ToolbarItem toolbarItem)
        {
            var image = await GetUIImage(toolbarItem.IconImageSource);

            if (image is null)
            {
                return new UIBarButtonItem(toolbarItem.Text, UIBarButtonItemStyle.Plain, (object sender, EventArgs e) => toolbarItem.Command?.Execute(toolbarItem.CommandParameter))
                {
                    AccessibilityIdentifier = toolbarItem.AutomationId
                };
            }
            else
            {
                return new UIBarButtonItem(image, UIBarButtonItemStyle.Plain, (object sender, EventArgs e) => toolbarItem.Command?.Execute(toolbarItem.CommandParameter))
                {
                    AccessibilityIdentifier = toolbarItem.AutomationId
                };
            }
        }

        static Task<UIImage?> GetUIImage(ImageSource source) => source switch
        {
            FileImageSource => new FileImageSourceHandler().LoadImageAsync(source),
            UriImageSource => new ImageLoaderSourceHandler().LoadImageAsync(source),
            StreamImageSource => new StreamImagesourceHandler().LoadImageAsync(source),
            _ => Task.FromResult<UIImage?>(null)
        };

        //Work-around to accommodate UISearchController height, https://github.com/brminnick/GitTrends/issues/171
        void HandleSizeChanged(object sender, EventArgs e)
        {
            if (ParentViewController?.NavigationItem.SearchController is not null
                && Element.Height is not -1
                && Element is Page page)
            {
                Element.SizeChanged -= HandleSizeChanged;

                if (NavigationController?.NavigationBar.PrefersLargeTitles is true)
                {
                    var statusBarSize = UIApplication.SharedApplication.StatusBarFrame.Size;
                    var statusBarHeight = Math.Min(statusBarSize.Height, statusBarSize.Width);

                    page.Padding = new Thickness(page.Padding.Left,
                                                    page.Padding.Top,
                                                    page.Padding.Right,
                                                    page.Padding.Bottom + statusBarHeight);
                }
            }
        }
    }
}
