using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views.InputMethods;
using GitTrends;
using GitTrends.Droid;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(RepositoryPage), typeof(SearchPageRenderer))]
namespace GitTrends.Droid
{
    public class SearchPageRenderer : PageRenderer
    {
        RepositoryPage? _repositoryPage;

        public SearchPageRenderer(Context context) : base(context)
        {

        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            if(Element is RepositoryPage repositoryPage)
            {
                _repositoryPage = repositoryPage;
                AddSearchToToolbar(repositoryPage);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (GetToolbar() is Toolbar toolBar)
                toolBar.Menu?.RemoveItem(Resource.Menu.MainMenu);

            base.Dispose(disposing);
        }

        void AddSearchToToolbar(in RepositoryPage repositoryPage)
        {
            if (GetToolbar() is Toolbar toolBar)
            {
                toolBar.Title = repositoryPage.Title;
                toolBar.InflateMenu(Resource.Menu.MainMenu);

                if (toolBar.Menu?.FindItem(Resource.Id.ActionSearch)?.ActionView?.JavaCast<SearchView>() is SearchView searchView)
                {
                    searchView.QueryTextChange += HandleQueryTextChange;
                    searchView.ImeOptions = (int)ImeAction.Search;
                    searchView.InputType = (int)InputTypes.TextVariationNormal;
                    searchView.MaxWidth = int.MaxValue; //Set to full width - http://stackoverflow.com/questions/31456102/searchview-doesnt-expand-full-width
                }
            }
        }

        void HandleQueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e) => _repositoryPage?.OnSearchBarTextChanged(e.NewText);

        Toolbar GetToolbar() => CrossCurrentActivity.Current.Activity.FindViewById<Toolbar>(Resource.Id.toolbar);
    }
}