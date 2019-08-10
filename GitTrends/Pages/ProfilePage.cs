using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace GitTrends
{
    public class ProfilePage : BaseContentPage<ProfileViewModel>
    {
        readonly FontAwesomeButton _gitHubLoginButton;
        readonly ActivityIndicator _activityIndicator;

        public ProfilePage(ProfileViewModel profileViewModel) : base("Settings", profileViewModel)
        {
            ViewModel.GitHubLoginUrlRetrieved += HandleGitHubLoginUrlRetrieved;

            var gitHubAvatarImage = new CircleImage
            {
                HeightRequest = 250,
                WidthRequest = 250,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
            };
            gitHubAvatarImage.SetBinding(CircleImage.SourceProperty, nameof(ViewModel.GitHubAvatarImageSource));

            var gitHubAliasLabel = new Label { HorizontalTextAlignment = TextAlignment.Center };
            gitHubAliasLabel.SetBinding(Label.TextProperty, nameof(ViewModel.GitHubAliasLabelText));

            _gitHubLoginButton = new FontAwesomeButton
            {
                TextColor = Color.White,
                FontSize = 24,
                BackgroundColor = ColorConstants.GitHubColor,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(10),
                Margin = new Thickness(25)
            };
            _gitHubLoginButton.SetBinding(IsEnabledProperty, nameof(ViewModel.IsNotAuthenticating));
            _gitHubLoginButton.SetBinding(Button.TextProperty, nameof(ViewModel.LoginButtonText));
            _gitHubLoginButton.SetBinding(Button.CommandProperty, nameof(ViewModel.LoginButtonCommand));

            _activityIndicator = new ActivityIndicator { Color = ColorConstants.DarkBlue };
            _activityIndicator.SetBinding(IsVisibleProperty, nameof(ViewModel.IsAuthenticating));
            _activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ViewModel.IsAuthenticating));

            Content = new StackLayout
            {
                Padding = new Thickness(20),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    gitHubAvatarImage,
                    gitHubAliasLabel,
                    _gitHubLoginButton,
                    _activityIndicator
                }
            };
        }

        protected override void OnAppearing()
        {
            ViewModel.IsAuthenticating = false;

            base.OnAppearing();
        }

        async void HandleGitHubLoginUrlRetrieved(object sender, string loginUrl) => await OpenBrowser(loginUrl);
    }
}
