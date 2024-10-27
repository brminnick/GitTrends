using CommunityToolkit.Mvvm.Input;
using GitTrends.Common;

namespace GitTrends;

public partial class AboutViewModel : BaseViewModel
{
	readonly DeepLinkingService _deepLinkingService;
	readonly GitTrendsStatisticsService _gitTrendsStatisticsService;

	public AboutViewModel(IDispatcher dispatcher,
							LibrariesService librariesService,
							IAnalyticsService analyticsService,
							DeepLinkingService deepLinkingService,
							GitTrendsStatisticsService gitTrendsStatisticsService) : base(analyticsService, dispatcher)
	{
		_deepLinkingService = deepLinkingService;
		_gitTrendsStatisticsService = gitTrendsStatisticsService;

		if (gitTrendsStatisticsService.Stars.HasValue)
			Stars = gitTrendsStatisticsService.Stars.Value;

		if (gitTrendsStatisticsService.Watchers.HasValue)
			Watchers = gitTrendsStatisticsService.Watchers.Value;

		if (gitTrendsStatisticsService.Forks.HasValue)
			Forks = gitTrendsStatisticsService.Forks.Value;

		InstalledLibraries = librariesService.InstalledLibraries;
		GitTrendsContributors = [.. gitTrendsStatisticsService.Contributors.OrderByDescending(static x => x.ContributionCount)];
	}

	public long? Stars { get; }
	public long? Forks { get; }
	public long? Watchers { get; }

	public IReadOnlyList<Contributor> GitTrendsContributors { get; }
	public IReadOnlyList<NuGetPackageModel> InstalledLibraries { get; }

	[RelayCommand]
	Task ViewOnGitHub(CancellationToken token)
	{
		if (_gitTrendsStatisticsService.GitHubUri is null)
			return Task.CompletedTask;

		AnalyticsService.Track("View On GitHub Tapped");

		return _deepLinkingService.OpenBrowser(_gitTrendsStatisticsService.GitHubUri, token);
	}

	[RelayCommand]
	Task RequestFeature(CancellationToken token)
	{
		if (_gitTrendsStatisticsService.GitHubUri is null)
			return Task.CompletedTask;

		AnalyticsService.Track("Request Feature Tapped");

		return _deepLinkingService.OpenBrowser(_gitTrendsStatisticsService.GitHubUri + "/issues/new?template=feature_request.md", token);
	}
}