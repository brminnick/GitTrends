using GitTrends.Mobile.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class CopyrightLabel : Label
    {
        public CopyrightLabel(IVersionTracking versionTracking)
        {
            this.BindTapGesture(nameof(SettingsViewModel.CopyrightLabelTappedCommand));

            AutomationId = SettingsPageAutomationIds.CopyrightLabel;

            LineBreakMode = LineBreakMode.WordWrap;

            VerticalOptions = LayoutOptions.EndAndExpand;
            HorizontalOptions = LayoutOptions.CenterAndExpand;

            VerticalTextAlignment = TextAlignment.End;
            HorizontalTextAlignment = TextAlignment.Center;

            FontSize = 12;
            FontFamily = FontFamilyConstants.RobotoMedium;

            LineHeight = 1.82;

            Opacity = 0.85;

#if DEBUG
            var versionNumberText = $"Version {versionTracking.CurrentVersion} (Debug)";
#elif RELEASE
            var versionNumberText = $"Version {versionTracking.CurrentVersion} (Release)";
#else
            var versionNumberText = $"Version {versionTracking.CurrentVersion}";
#endif

            Text = $"{versionNumberText}\nCreated by Code Traveler LLC";

            SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
        }
    }
}
