using System;
using GitTrends.Mobile.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace GitTrends
{
    class GitHubSettingsView : ContentView
    {
        public GitHubSettingsView()
        {
            const int _imageHeight = 200;
            const int _demoButtonFontSize = 8;

            var gitHubAvatarImage = new CircleImage
            {
                AutomationId = SettingsPageAutomationIds.GitHubAvatarImage,
                HeightRequest = _imageHeight,
                WidthRequest = _imageHeight,
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
                Padding = new Thickness(2),
                BackgroundColor = Color.Transparent,
                FontSize = _demoButtonFontSize,
                Text = "Enter Demo Mode",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
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

            var relativeLayout = new RelativeLayout();

            relativeLayout.Children.Add(gitHubAvatarImage,
                //Center the image horizontally within the RelativeLayout
                xConstraint: Constraint.RelativeToParent(parent => parent.Width / 2 - getImageSizeConstraint(parent) / 2),
                //Pin the image to the top of the screen
                yConstraint: Constraint.Constant(0),
                //Width and Height should be the same
                widthConstraint: Constraint.RelativeToParent(parent => getImageSizeConstraint(parent)),
                //Width and Height should be the same
                heightConstraint: Constraint.RelativeToParent(parent => getImageSizeConstraint(parent)));

            relativeLayout.Children.Add(gitHubLoginButton,
                //Center the button horizontally within the RelativeLayout
                xConstraint: Constraint.RelativeToParent(parent => parent.Width / 2 - getWidth(parent, gitHubLoginButton) / 2),
                //Place the button below gitHubAvatarImage
                yConstraint: Constraint.RelativeToView(gitHubAvatarImage, (parent, view) => view.Y + view.Height + 5),
                //Ensure the button scales to the height of the RelativeLayout
                heightConstraint: Constraint.RelativeToParent(parent => getLoginButtonSizeConstraint(parent)));

            relativeLayout.Children.Add(demoButton,
                //Center the button horizontally within the RelativeLayout
                xConstraint: Constraint.RelativeToParent(parent => parent.Width / 2 - getWidth(parent, demoButton) / 2),
                //Place the button below gitHubLoginButton
                yConstraint: Constraint.RelativeToView(gitHubLoginButton, (parent, view) => view.Y + view.Height + 2),
                heightConstraint: Constraint.Constant(getDemoButtonSizeConstraint()));

            relativeLayout.Children.Add(activityIndicator,
                //Center the activityIndicator horizontally within the RelativeLayout
                xConstraint: Constraint.RelativeToParent(parent => parent.Width / 2 - getWidth(parent, activityIndicator) / 2),
                //Place the activityIndicator below gitHubLoginButton
                yConstraint: Constraint.RelativeToView(gitHubLoginButton, (parent, view) => view.Y + view.Height + 5));

            Content = relativeLayout;

            static double getWidth(in RelativeLayout parent, in View view) => view.Measure(parent.Width, parent.Height).Request.Width;

            static double getImageSizeConstraint(RelativeLayout relativeLayout)
            {
                var maximimumImageSize = Math.Min(relativeLayout.Width, relativeLayout.Height) / 1.75;
                return Math.Min(_imageHeight, maximimumImageSize);
            }

            static double getLoginButtonSizeConstraint(RelativeLayout relativeLayout)
            {
                var maximimumButtonSize = relativeLayout.Height / 2.25;
                return Math.Min(75, maximimumButtonSize);
            }

            static double getDemoButtonSizeConstraint() => _demoButtonFontSize + 10;
        }
    }
}