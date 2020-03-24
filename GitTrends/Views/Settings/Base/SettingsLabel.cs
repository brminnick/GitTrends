using Xamarin.Forms;

namespace GitTrends
{
    class SettingsLabel : Label
    {
        public SettingsLabel(string text, string automationId)
        {
            AutomationId = automationId;
            Text = text;
            FontAttributes = FontAttributes.Bold;
            FontSize = 18;
            SetDynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
        }
    }
}
