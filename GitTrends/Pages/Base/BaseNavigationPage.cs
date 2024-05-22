using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace GitTrends;

public class BaseNavigationPage : Microsoft.Maui.Controls.NavigationPage
{
	public BaseNavigationPage(Microsoft.Maui.Controls.Page root) : base(root)
	{
		this.DynamicResources((BarTextColorProperty, nameof(BaseTheme.NavigationBarTextColor)),
								(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor)),
								(BarBackgroundColorProperty, nameof(BaseTheme.NavigationBarBackgroundColor)));

		On<iOS>().SetPrefersLargeTitles(true);
		On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
	}
}