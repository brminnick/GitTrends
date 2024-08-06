using System.Diagnostics;

namespace GitTrends;

partial class AppShell : Shell
{
	public AppShell(SplashScreenPage splashScreenPage)
	{
		Items.Add(splashScreenPage);
		
		SetDynamicResource(ForegroundColorProperty, nameof(BaseTheme.NavigationBarTextColor));
		SetDynamicResource(BackgroundColorProperty,nameof(BaseTheme.PageBackgroundColor));
		SetDynamicResource(TitleColorProperty,nameof(BaseTheme.NavigationBarBackgroundColor));

		Navigating += HandleNavigating;
	}

	static void HandleNavigating(object? sender, ShellNavigatingEventArgs e)
	{
		Trace.WriteLine($"Navigating from: {e.Current?.Location}");
		Trace.WriteLine($"Navigating to: {e.Target}");
	}
}