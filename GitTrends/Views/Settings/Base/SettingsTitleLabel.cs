using Xamarin.Forms;

namespace GitTrends
{
    abstract class SettingsTitleLabel : Label
    {
        protected SettingsTitleLabel()
        {
            FontSize = 14;

            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.CenterAndExpand;

            VerticalTextAlignment = TextAlignment.Center;

            FontFamily = FontFamilyConstants.RobotoMedium;
            LineBreakMode = LineBreakMode.TailTruncation;

            SetDynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
        }
    }
}
