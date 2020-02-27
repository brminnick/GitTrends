using GitTrends.Mobile.Shared;
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
                AutomationId = SettingsPageAutomationIds.GitHubAvatarImage,
                HeightRequest = 200,
                WidthRequest = 200,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Aspect = Aspect.AspectFit
            };
            gitHubAvatarImage.SetBinding(CircleImage.SourceProperty, nameof(SettingsViewModel.GitHubAvatarImageSource));

            var gitHubAliasLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                AutomationId = SettingsPageAutomationIds.GitHubAliasLabel,
            };
            gitHubAliasLabel.SetBinding(Label.TextProperty, nameof(SettingsViewModel.GitHubAliasLabelText));

            var gitHubLoginButton = new FontAwesomeButton
            {
                AutomationId = SettingsPageAutomationIds.GitHubLoginButton,
                TextColor = Color.White,
                FontSize = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 25, 0, 5)
            };
            gitHubLoginButton.SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.ButtonBackgroundColor));
            gitHubLoginButton.SetBinding(Button.TextProperty, nameof(SettingsViewModel.LoginButtonText));
            gitHubLoginButton.SetBinding(Button.CommandProperty, nameof(SettingsViewModel.LoginButtonCommand));

            var demoButton = new Button
            {
                FontSize = 8,
                Text = "Enable Demo Mode"
            };
            demoButton.SetDynamicResource(Button.TextColorProperty, nameof(BaseTheme.TextColor));
            demoButton.SetBinding(IsVisibleProperty, nameof(SettingsViewModel.IsDemoButtonVisible));
            demoButton.SetBinding(Button.CommandProperty, nameof(SettingsViewModel.DemoButtonCommand));

            var activityIndicator = new ActivityIndicator
            {
                AutomationId = SettingsPageAutomationIds.GitHubSettingsViewActivityIndicator,
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
                    new RowDefinition { Height = new GridLength(10, GridUnitType.Absolute) },
                    new RowDefinition { Height = new GridLength(20, GridUnitType.Absolute) }
                },

                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };

            grid.Children.Add(gitHubAvatarImage, 0, 0);
            grid.Children.Add(gitHubLoginButton, 0, 1);
            grid.Children.Add(demoButton, 0, 2);
            grid.Children.Add(activityIndicator, 0, 3);

            Content = grid;
        }
    }
}