using Xamarin.Forms;

namespace GitTrends
{
    public class SettingsButton : Button
    {
        public SettingsButton(string text, string automationId)
        {
            Text = text;
            AutomationId = automationId;
            Padding = new Thickness(10, 0);
            FontSize = 14;
            BorderWidth = Device.RuntimePlatform is Device.Android ? 0.75 : 1;

            //Use the default corner radius on iOS
            CornerRadius = Device.RuntimePlatform is Device.Android ? 7 : -1;

            SetDynamicResource(TextColorProperty, nameof(BaseTheme.SettingsButtonFontColor));
            SetDynamicResource(BorderColorProperty, nameof(BaseTheme.SettingsButtonBorderColor));
            SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
        }
    }
}
