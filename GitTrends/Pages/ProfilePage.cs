using System;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace GitTrends
{
    public class ProfilePage : BaseContentPage<ProfileViewModel>
    {
        #region Constant Fields
        readonly FontAwesomeButton _gitHubLoginButton;
        #endregion

        public ProfilePage() : base("Settings")
        {
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
                Text = FontAwesomeButton.GitHubOctocat.ToString() + " Connect with GitHub",
                TextColor = Color.White,
                FontSize = 24,
                BackgroundColor = ColorConstants.GitHubColor,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(10),
                Margin = new Thickness(25)
            };
            _gitHubLoginButton.Clicked += HandleGitHubLoginButtonClicked;

            Content = new StackLayout
            {
                Padding = new Thickness(20),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    gitHubAvatarImage,
                    gitHubAliasLabel,
                    _gitHubLoginButton
                }
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _gitHubLoginButton.IsEnabled = true;
        }

        async void HandleGitHubLoginButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button loginButton)
                loginButton.IsEnabled = false;

            var loginUrl = await GitHubAuthenticationService.GetGitHubLoginUrl().ConfigureAwait(false);
            Device.BeginInvokeOnMainThread(async () => await OpenBrowser(loginUrl).ConfigureAwait(false));
        }
    }
}
