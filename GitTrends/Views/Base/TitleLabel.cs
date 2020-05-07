using Xamarin.Forms;

namespace GitTrends
{
    abstract class TitleLabel : Label
    {
        protected TitleLabel()
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
