using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends.Views.Settings
{
    public class AppSettingsView : StackLayout
    {
        public AppSettingsView()
        {
            Spacing = 0;

            var logoutSetting = new SettingsComponent("logout.svg", "Logout", false);
            logoutSetting.BindTapGesture(nameof(SettingsViewModel.ConnectToGitHubButtonCommand));

            var notificationSetting = new SettingsComponent("bell.svg", "Register for Push Notifications", true);
            var themeSetting = new SettingsComponent("darkmode.svg", "Dark Mode", true);

            Children.Add(new Separator().Margin(new Thickness(32, 24, 32, 16)));
            Children.Add(logoutSetting);
            Children.Add(new Separator().Margin(new Thickness(32, 16)));
            Children.Add(notificationSetting);
            Children.Add(new Separator().Margin(new Thickness(32, 16)));
            Children.Add(new SettingsHeadingLabel("THEME").Margin(new Thickness(32, 0)));
            Children.Add(themeSetting);
        }
    }

    class SettingsComponent : StackLayout
    {
        public SettingsComponent(in string svgIconPath, in string text, in bool isToggleSetting)
        {
            Orientation = StackOrientation.Horizontal;
            Spacing = 16;

            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.CenterAndExpand;

            HeightRequest = 32;
            Margin = new Thickness(32, 0);

            Children.Add(new SettingsSVGImage(svgIconPath, nameof(BaseTheme.IconColor), 24, 24).Start());
            Children.Add(new SettingsTitleLabel(text));

            if (isToggleSetting)
                Children.Add(new SettingsSwitch().End());
            else
            {
                var rightArrow = new SettingsSVGImage("right_arrow.svg", nameof(BaseTheme.TextColor), 24, 24).End();
                rightArrow.Opacity = 0.5;

                Children.Add(rightArrow);
            }
        }
    }

    class SettingsHeadingLabel : PrimaryColorLabel
    {
        public SettingsHeadingLabel(in string text) : base(12, text)
        {
            CharacterSpacing = 1.56;
            HorizontalOptions = LayoutOptions.FillAndExpand;
            FontFamily = FontFamilyConstants.RobotoMedium;

            SetDynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
        }
    }

    class SettingsTitleLabel : PrimaryColorLabel
    {
        public SettingsTitleLabel(in string text) : base(14, text)
        {
            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.CenterAndExpand;
            FontFamily = FontFamilyConstants.RobotoMedium;
        }
    }

    abstract class PrimaryColorLabel : Label
    {
        protected PrimaryColorLabel(in double fontSize, in string text)
        {
            FontSize = fontSize;
            Text = text;
            LineBreakMode = LineBreakMode.TailTruncation;

            SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
        }
    }

    class SettingsSVGImage : SvgImage
    {
        public SettingsSVGImage(in string svgFileName, string baseThemeColor, in double widthRequest = default, in double heightRequest = default)
            : base(svgFileName, () => (Color)Application.Current.Resources[baseThemeColor], widthRequest, heightRequest)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand;
            HorizontalOptions = LayoutOptions.CenterAndExpand;
        }
    }


    class SettingsSwitch : Switch
    {
        public SettingsSwitch()
        {
            if (Device.RuntimePlatform is Device.iOS) SetDynamicResource(OnColorProperty, nameof(BaseTheme.PrimaryColor));
        }
    }

    class Separator : BoxView
    {
        public Separator()
        {
            HeightRequest = 1;
            SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
        }
    }
}
