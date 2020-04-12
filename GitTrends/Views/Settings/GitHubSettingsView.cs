using GitTrends.Mobile.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class GitHubUserView : Grid
    {
        public const int TotalHeight = _imageHeight + _nameLabelHeight + _aliasLabelHeight + _margin * 2;

        const int _imageHeight = 140;
        const int _margin = 16;
        const int _nameLabelHeight = 28;
        const int _aliasLabelHeight = 18;

        public GitHubUserView()
        {
            Margin = new Thickness(_margin);
            RowSpacing = 6;

            RowDefinitions = Rows.Define(
                (Row.Image, AbsoluteGridLength(_imageHeight)),
                (Row.Name, AbsoluteGridLength(_nameLabelHeight)),
                (Row.Alias, AbsoluteGridLength(_aliasLabelHeight)));

            Children.Add(new GitHubAvatarImage().Row(Row.Image));
            Children.Add(new NameLabel().Row(Row.Name));
            Children.Add(new AliasLabel().Row(Row.Alias));
            Children.Add(new IsAuthenticatingActivityIndicator().Row(Row.Name).RowSpan(2));
        }

        enum Row { Image, Name, Alias }

        class GitHubAvatarImage : CircleImage
        {
            public GitHubAvatarImage()
            {
                AutomationId = SettingsPageAutomationIds.GitHubAvatarImage;
                HeightRequest = _imageHeight;
                WidthRequest = _imageHeight;
                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Center;
                Aspect = Aspect.AspectFit;

                this.SetBinding(SourceProperty, nameof(SettingsViewModel.GitHubAvatarImageSource));
            }
        }

        class NameLabel : Label
        {
            public NameLabel()
            {
                FontSize = _nameLabelHeight - 2;
                HorizontalOptions = LayoutOptions.CenterAndExpand;
                FontFamily = FontFamilyConstants.RobotoMedium;

                this.SetBinding(TextProperty, nameof(SettingsViewModel.GitHubNameLabelText));
                this.SetBinding(IsVisibleProperty, nameof(SettingsViewModel.IsNotAuthenticating));

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.GitHubHandleColor));
            }
        }

        class AliasLabel : Label
        {
            public AliasLabel()
            {
                FontSize = _aliasLabelHeight - 2;
                HorizontalOptions = LayoutOptions.CenterAndExpand;
                FontFamily = FontFamilyConstants.RobotoRegular;
                Opacity = 0.60;

                this.SetBinding(TextProperty, nameof(SettingsViewModel.GitHubAliasLabelText));
                this.SetBinding(IsVisibleProperty, nameof(SettingsViewModel.IsNotAuthenticating));

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.GitHubHandleColor));
            }
        }

        class IsAuthenticatingActivityIndicator : ActivityIndicator
        {
            public IsAuthenticatingActivityIndicator()
            {
                AutomationId = SettingsPageAutomationIds.GitHubSettingsViewActivityIndicator;

                this.Center();

                this.SetBinding(IsVisibleProperty, nameof(SettingsViewModel.IsAuthenticating));
                this.SetBinding(IsRunningProperty, nameof(SettingsViewModel.IsAuthenticating));

                SetDynamicResource(ColorProperty, nameof(BaseTheme.ActivityIndicatorColor));
            }
        }
    }
}