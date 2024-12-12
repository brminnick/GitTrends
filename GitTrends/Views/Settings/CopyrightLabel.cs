using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;

namespace GitTrends;

class CopyrightLabel : Label
{
	public CopyrightLabel()
	{
		Opacity = 0.85;

		FontSize = 12;
		LineHeight = 1.82;
		FontFamily = FontFamilyConstants.RobotoMedium;
		LineBreakMode = LineBreakMode.WordWrap;

		VerticalOptions = LayoutOptions.End;
		HorizontalOptions = LayoutOptions.Center;

		VerticalTextAlignment = TextAlignment.End;
		HorizontalTextAlignment = TextAlignment.Center;

		AutomationId = SettingsPageAutomationIds.CopyrightLabel;

		this.Bind(TextProperty,
				getter: static (SettingsViewModel vm) => vm.CopyrightLabelText,
				mode: BindingMode.OneTime)
			.BindTapGesture(nameof(SettingsViewModel.CopyrightLabelTappedCommand))
			.DynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
	}
}