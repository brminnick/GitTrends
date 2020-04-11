using GitTrends.Mobile.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace GitTrends
{
    class GitHubSettingsView : StackLayout
    {
        const int _imageHeight = 140;
        const int _imageMargin = 16;
        const int _nameLabelHeight = 20;
        const int _aliasLabelHeight = 12;

        public GitHubSettingsView()
        {
            HeightRequest = _imageHeight + _nameLabelHeight + _aliasLabelHeight + _imageMargin * 2;

            Spacing = 0;

            var activityIndicator = new ActivityIndicator
            {
                AutomationId = SettingsPageAutomationIds.GitHubSettingsViewActivityIndicator,
                VerticalOptions = LayoutOptions.Start
            };
            activityIndicator.SetDynamicResource(ActivityIndicator.ColorProperty, nameof(BaseTheme.ActivityIndicatorColor));
            activityIndicator.SetBinding(IsVisibleProperty, nameof(SettingsViewModel.IsAuthenticating));
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(SettingsViewModel.IsAuthenticating));

            Children.Add(new GitHubAvatarImage());
            Children.Add(new NameLabel());
            Children.Add(new AliasLabel());
        }

        class GitHubAvatarImage : CircleImage
        {
            public GitHubAvatarImage()
            {
                AutomationId = SettingsPageAutomationIds.GitHubAvatarImage;
                HeightRequest = _imageHeight;
                WidthRequest = _imageHeight;
                Margin = new Thickness(_imageMargin);
                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Center;
                Aspect = Aspect.AspectFit;

                this.SetBinding(CircleImage.SourceProperty, nameof(SettingsViewModel.GitHubAvatarImageSource));
            }
        }

        class NameLabel : Label
        {
            public NameLabel()
            {
                FontSize = _nameLabelHeight;
                HorizontalOptions = LayoutOptions.CenterAndExpand;
                FontFamily = FontFamilyConstants.RobotoMedium;

                this.SetBinding(TextProperty, nameof(SettingsViewModel.GitHubNameLabelText));

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.GitHubHandleColor));
            }
        }

        class AliasLabel : Label
        {
            public AliasLabel()
            {
                FontSize = _aliasLabelHeight;
                HorizontalOptions = LayoutOptions.CenterAndExpand;
                FontFamily = FontFamilyConstants.RobotoRegular;
                Opacity = 0.60;

                this.SetBinding(Label.TextProperty, nameof(SettingsViewModel.GitHubAliasLabelText));

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.GitHubHandleColor));
            }
        }
    }
}