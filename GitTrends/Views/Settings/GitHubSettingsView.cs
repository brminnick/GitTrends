using System;
using GitTrends.Mobile.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace GitTrends
{
    class GitHubSettingsView : ContentView
    {
        const int _imageHeight = 140;

        public GitHubSettingsView()
        {

            var gitHubAvatarImage = new GitHubAvatarImage();
            gitHubAvatarImage.SetBinding(CircleImage.SourceProperty, nameof(SettingsViewModel.GitHubAvatarImageSource));

            var gitHubNameLabel = new NameLabel();
            gitHubNameLabel.SetBinding(Label.TextProperty, nameof(SettingsViewModel.GitHubNameLabelText));

            var gitHubAliasLabel = new AliasLabel();
            gitHubAliasLabel.SetBinding(Label.TextProperty, nameof(SettingsViewModel.GitHubAliasLabelText));


            var activityIndicator = new ActivityIndicator
            {
                AutomationId = SettingsPageAutomationIds.GitHubSettingsViewActivityIndicator,
                VerticalOptions = LayoutOptions.Start
            };
            activityIndicator.SetDynamicResource(ActivityIndicator.ColorProperty, nameof(BaseTheme.ActivityIndicatorColor));
            activityIndicator.SetBinding(IsVisibleProperty, nameof(SettingsViewModel.IsAuthenticating));
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(SettingsViewModel.IsAuthenticating));

            Content = new StackLayout()
            {
                Orientation = StackOrientation.Vertical,
                Spacing = 0,
                Children =
                {
                    gitHubAvatarImage,
                    gitHubNameLabel,
                    gitHubAliasLabel,
                }

            };
        }

        class GitHubAvatarImage : CircleImage
        {
            public GitHubAvatarImage()
            {
                AutomationId = SettingsPageAutomationIds.GitHubAvatarImage;
                HeightRequest = _imageHeight;
                WidthRequest = _imageHeight;
                Margin = new Thickness(16);
                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Center;
                Aspect = Aspect.AspectFit;

            }
        }

        class NameLabel : Label
        {
            public NameLabel()
            {
                FontSize = 20;
                HorizontalOptions = LayoutOptions.CenterAndExpand;
                FontFamily = FontFamilyConstants.RobotoMedium;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.GitHubHandleColor));
            }
        }

        class AliasLabel : Label
        {
            public AliasLabel()
            {
                FontSize = 12;
                HorizontalOptions = LayoutOptions.CenterAndExpand;
                FontFamily = FontFamilyConstants.RobotoRegular;
                Opacity = 0.60;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.GitHubHandleColor));
            }
        }
    }
}