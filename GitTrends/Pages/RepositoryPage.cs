using System;
using System.Linq;
using GitTrends.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class RepositoryPage : BaseContentPage<RepositoryViewModel>
    {
        #region Constant Fields
        readonly ListView _listView;
        #endregion

        #region Constructors
        public RepositoryPage()
        {
            ViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;

            var gitHubLoginButton = new Button { Text = "Login with GitHub" };
            gitHubLoginButton.SetBinding(Button.CommandProperty, nameof(ViewModel.LoginButtonCommand));
            gitHubLoginButton.SetBinding(IsVisibleProperty, nameof(ViewModel.IsGitHubLoginButtonVisible));

            _listView = new ListView(ListViewCachingStrategy.RecycleElement)
            {
                IsPullToRefreshEnabled = true,
                ItemTemplate = new DataTemplate(typeof(RepositoryViewCell)),
                SeparatorVisibility = SeparatorVisibility.None,
                RowHeight = RepositoryViewCell.ImageHeight,
                RefreshControlColor = ColorConstants.DarkBlue,
                BackgroundColor = Color.Transparent,
                SelectionMode = ListViewSelectionMode.None
            };
            _listView.ItemTapped += HandleListViewItemTapped;
            _listView.SetBinding(IsVisibleProperty, nameof(ViewModel.IsListViewVisible));
            _listView.SetBinding(ListView.IsRefreshingProperty, nameof(ViewModel.IsRefreshing));
            _listView.SetBinding(ListView.ItemsSourceProperty, nameof(ViewModel.RepositoryCollection));
            _listView.SetBinding(ListView.RefreshCommandProperty, nameof(ViewModel.PullToRefreshCommand));

            var settingsToolbarItem = new ToolbarItem { IconImageSource = "Settings" };
            settingsToolbarItem.Clicked += HandleSettingsToolbarItem;
            ToolbarItems.Add(settingsToolbarItem);

            Title = "Repositories";
            BackgroundColor = ColorConstants.LightBlue;

            var absoluteLayout = new AbsoluteLayout();
            absoluteLayout.Children.Add(gitHubLoginButton, new Rectangle(0.5, 0.5, -1, -1), AbsoluteLayoutFlags.PositionProportional);
            absoluteLayout.Children.Add(_listView, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

            Content = absoluteLayout;
        }
        #endregion

        #region Methods
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var token = await GitHubAuthenticationService.GetGitHubToken();
            if (token?.AccessToken != null)
            {
                _listView.BeginRefresh();
            }
        }

        async void HandleListViewItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (sender is ListView listView)
                listView.SelectedItem = null;

            if (e.Item is Repository repository
                && repository.Uri.IsAbsoluteUri
                && repository.Uri.Scheme.Equals(Uri.UriSchemeHttps))
            {
                var browserOptions = new BrowserLaunchOptions
                {
                    PreferredToolbarColor = ColorConstants.LightBlue,
                    PreferredControlColor = Color.DarkBlue
                };

                await Browser.OpenAsync(repository.Uri, browserOptions);
            }
        }

        void HandleSettingsToolbarItem(object sender, EventArgs e) => throw new NotImplementedException();

        void HandlePullToRefreshFailed(object sender, PullToRefreshFailedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (!Application.Current.MainPage.Navigation.ModalStack.Any()
                    && Application.Current.MainPage.Navigation.NavigationStack.Last() is RepositoryPage)
                {
                    await DisplayAlert(e.ErrorTitle, e.ErrorMessage, "OK");
                }
            });
        }
        #endregion
    }
}
