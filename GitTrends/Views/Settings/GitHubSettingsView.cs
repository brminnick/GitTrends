using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace GitTrends
{
    public class GitHubSettingsView : ContentView
    {
        public GitHubSettingsView()
        {
            var gitHubAvatarImage = new CircleImage
            {
                HeightRequest = 200,
                WidthRequest = 200,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
            };
            gitHubAvatarImage.SetBinding(CircleImage.SourceProperty, nameof(SettingsViewModel.GitHubAvatarImageSource));

            var gitHubAliasLabel = new Label { HorizontalTextAlignment = TextAlignment.Center };
            gitHubAliasLabel.SetBinding(Label.TextProperty, nameof(SettingsViewModel.GitHubAliasLabelText));

            var gitHubLoginButton = new FontAwesomeButton
            {
                TextColor = Color.White,
                FontSize = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 25, 0, 5)
            };
            gitHubLoginButton.SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.ButtonBackgroundColor));
            gitHubLoginButton.SetBinding(IsEnabledProperty, nameof(SettingsViewModel.IsNotAuthenticating));
            gitHubLoginButton.SetBinding(Button.TextProperty, nameof(SettingsViewModel.LoginButtonText));
            gitHubLoginButton.SetBinding(Button.CommandProperty, nameof(SettingsViewModel.LoginButtonCommand));

            var activityIndicator = new ActivityIndicator
            {
                VerticalOptions = LayoutOptions.Start
            };
            activityIndicator.SetDynamicResource(ActivityIndicator.ColorProperty, nameof(BaseTheme.RefreshControlColor));
            activityIndicator.SetBinding(IsVisibleProperty, nameof(SettingsViewModel.IsAuthenticating));
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(SettingsViewModel.IsAuthenticating));

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(7, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(20, GridUnitType.Absolute) }
                },

                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };

            grid.Children.Add(gitHubAvatarImage, 0, 0);
            grid.Children.Add(gitHubLoginButton, 0, 1);
            grid.Children.Add(activityIndicator, 0, 2);

            Content = grid;
        }
    }
}
