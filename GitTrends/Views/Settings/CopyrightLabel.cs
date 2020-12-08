using GitTrends.Mobile.Common;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;

namespace GitTrends
{
    class CopyrightLabel : Label
    {
        public CopyrightLabel()
        {
            Opacity = 0.85;

            FontSize = 12;
            LineHeight = 1.82;
            FontFamily = FontFamilyConstants.RobotoMedium;
            LineBreakMode = LineBreakMode.WordWrap;

            VerticalOptions = LayoutOptions.EndAndExpand;
            HorizontalOptions = LayoutOptions.CenterAndExpand;

            VerticalTextAlignment = TextAlignment.End;
            HorizontalTextAlignment = TextAlignment.Center;            

            AutomationId = SettingsPageAutomationIds.CopyrightLabel;

            this.Bind(nameof(SettingsViewModel.CopyrightLabelText))
                .BindTapGesture(nameof(SettingsViewModel.CopyrightLabelTappedCommand))
                .DynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
        }
    }
}
