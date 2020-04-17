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
        public const int TotalHeight = _imageHeight + _nameLabelHeight + _aliasLabelHeight + _topMargin + _bottomMargin + _rowSpacing * 3;

        const int _imageHeight = 140;
        const int _topMargin = 16;
        const int _bottomMargin = 8;
        const int _widthMargin = 16;
        const int _nameLabelHeight = 24;
        const int _aliasLabelHeight = 18;
        const int _rowSpacing = 4;

        public GitHubUserView()
        {
            AutomationId = SettingsPageAutomationIds.GitHubUserView;

            Margin = new Thickness(_widthMargin, _topMargin, _widthMargin, _bottomMargin);
            RowSpacing = 4;

            RowDefinitions = Rows.Define(
                (Row.Image, AbsoluteGridLength(_imageHeight)),
                (Row.Name, AbsoluteGridLength(_nameLabelHeight)),
                (Row.Alias, AbsoluteGridLength(_aliasLabelHeight)));

            Children.Add(new GitHubAvatarImage().Row(Row.Image));
            Children.Add(new NameLabel().Row(Row.Name));
            Children.Add(new IsAuthenticatingActivityIndicator().Row(Row.Name).RowSpan(2));
            Children.Add(new AliasLabel().Row(Row.Alias));
            Children.Add(new TryDemoButton().Row(Row.Alias));

            this.BindTapGesture(nameof(SettingsViewModel.GitHubUserViewTappedCommand));
        }

        enum Row { Image, Name, Alias }

        class GitHubAvatarImage : CircleImage
        {
            public GitHubAvatarImage()
            {
                this.Center();

                HeightRequest = _imageHeight;
                WidthRequest = _imageHeight;
                Aspect = Aspect.AspectFit;

                AutomationId = SettingsPageAutomationIds.GitHubAvatarImage;

                this.SetBinding(SourceProperty, nameof(SettingsViewModel.GitHubAvatarImageSource));
            }
        }

        class NameLabel : Label
        {
            public NameLabel()
            {
                this.Center();

                AutomationId = SettingsPageAutomationIds.GitHubNameLabel;

                FontSize = _nameLabelHeight - 6;
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
                this.Center();
                Opacity = 0.6;

                AutomationId = SettingsPageAutomationIds.GitHubAliasLabel;

                FontSize = _aliasLabelHeight - 4;
                FontFamily = FontFamilyConstants.RobotoRegular;

                this.SetBinding(TextProperty, nameof(SettingsViewModel.GitHubAliasLabelText));
                this.SetBinding(IsVisibleProperty, nameof(SettingsViewModel.IsAliasLabelVisible));

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.GitHubHandleColor));
            }
        }

        class TryDemoButton : Button
        {
            public TryDemoButton()
            {
                this.Center();
                Opacity = 0.6;

                AutomationId = SettingsPageAutomationIds.DemoModeButton;

                FontSize = _aliasLabelHeight - 4;
                FontFamily = FontFamilyConstants.RobotoRegular;
                Text = GitHubLoginButtonConstants.TryDemo;

                this.SetBinding(IsVisibleProperty, nameof(SettingsViewModel.IsDemoButtonVisible));
                this.SetBinding(CommandProperty, nameof(SettingsViewModel.DemoButtonCommand));

                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
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