using CommunityToolkit.Maui.Markup;

namespace GitTrends;

class BorderButton : BounceButton
{
	public BorderButton(in string automationId, in IDeviceInfo deviceInfo)
	{
		AutomationId = automationId;

		FontSize = 14;
		FontFamily = FontFamilyConstants.RobotoMedium;

		Padding = new Thickness(10, 0);
		BorderWidth = deviceInfo.Platform == DevicePlatform.Android ? 0.75 : 1;

		//Use the default corner radius on iOS
		CornerRadius = deviceInfo.Platform == DevicePlatform.Android ? 7 : -1;


		this.DynamicResources((TextColorProperty, nameof(BaseTheme.BorderButtonFontColor)),
			(BorderColorProperty, nameof(BaseTheme.BorderButtonBorderColor)),
			(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor)));
	}
}