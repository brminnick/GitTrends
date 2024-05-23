using CommunityToolkit.Maui.Markup;

namespace GitTrends;

abstract class SettingsSwitch : Switch
{
	protected SettingsSwitch(in IDeviceInfo deviceInfo)
	{
		HorizontalOptions = LayoutOptions.End;

		if (deviceInfo.Platform == DevicePlatform.iOS)
			this.DynamicResource(OnColorProperty, nameof(BaseTheme.PrimaryColor));
	}
}