using CommunityToolkit.Maui.Markup;

namespace GitTrends;

abstract class TitleLabel : Label
{
	protected TitleLabel()
	{
		FontSize = 14;

		HorizontalOptions = LayoutOptions.Fill;
		VerticalOptions = LayoutOptions.Center;

		VerticalTextAlignment = TextAlignment.Center;

		FontFamily = FontFamilyConstants.RobotoMedium;
		LineBreakMode = LineBreakMode.TailTruncation;

		this.DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
	}
}