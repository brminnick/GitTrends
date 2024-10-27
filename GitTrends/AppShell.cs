using System.Diagnostics;

namespace GitTrends;

partial class AppShell : Shell
{
	public AppShell(SplashScreenPage splashScreenPage)
	{
		Items.Add(splashScreenPage);

		SetDynamicResource(ForegroundColorProperty, nameof(BaseTheme.NavigationBarTextColor));
		SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.NavigationBarBackgroundColor));
		SetDynamicResource(FlyoutBackgroundColorProperty, nameof(BaseTheme.NavigationBarBackgroundColor));

		Navigating += HandleNavigating;

#if IOS || MACCATALYST
		ShellAttachedProperties.SetPrefersLargeTitles(this, true);
#endif
	}

	static void HandleNavigating(object? sender, ShellNavigatingEventArgs e)
	{
		Trace.WriteLine($"Navigating from: {e.Current?.Location}");
		Trace.WriteLine($"Navigating to: {e.Target}");
	}
}