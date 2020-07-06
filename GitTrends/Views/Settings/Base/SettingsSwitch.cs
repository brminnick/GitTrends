using Xamarin.Forms;

namespace GitTrends
{
    abstract class SettingsSwitch : Switch
    {
        public SettingsSwitch()
        {
            HorizontalOptions = LayoutOptions.End;

            if (Device.RuntimePlatform is Device.iOS)
                this.DynamicResource(OnColorProperty, nameof(BaseTheme.PrimaryColor));
        }
    }
}
