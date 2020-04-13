using GitTrends.Mobile.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class CopyrightLabel : Label
    {
        public CopyrightLabel()
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

#if AppStore
                var versionNumberText = $"Version {VersionTracking.CurrentVersion}";
#elif RELEASE
                var versionNumberText = $"Version {VersionTracking.CurrentVersion} (Release)";
#elif DEBUG
            var versionNumberText = $"Version {VersionTracking.CurrentVersion} (Debug)";
#endif

            Text = $"{versionNumberText}\nCreated by Code Traveler LLC";

            SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
        }
    }
}
