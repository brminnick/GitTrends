namespace GitTrends;

partial class AppShell
{
	public static string GetPageRoute<TPage>() where TPage : BaseContentPage
	{
		var pageType = typeof(TPage);

		if (pageType == typeof(SplashScreenPage))
			return $"//{nameof(SplashScreenPage)}";
		
		if (pageType == typeof(RepositoryPage))
			return $"//{nameof(RepositoryPage)}";
		
		if (pageType == typeof(TrendsCarouselPage))
			return $"//{nameof(RepositoryPage)}/{nameof(TrendsCarouselPage)}";
		
		if (pageType == typeof(ReferringSitesPage))
			return $"//{nameof(RepositoryPage)}/{nameof(TrendsCarouselPage)}/{nameof(ReferringSitesPage)}";
		
		if (pageType == typeof(SettingsPage))
			return $"//{nameof(RepositoryPage)}/{nameof(SettingsPage)}";
		
		if (pageType == typeof(AboutPage))
			return $"//{nameof(RepositoryPage)}/{nameof(SettingsPage)}/{nameof(AboutPage)}";

		throw new NotImplementedException("Page Route Not Implemented");
	}
}