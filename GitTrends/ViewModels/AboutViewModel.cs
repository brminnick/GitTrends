using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public partial class AboutViewModel : BaseViewModel
	{
		readonly DeepLinkingService _deepLinkingService;
		readonly GitTrendsStatisticsService _gitTrendsStatisticsService;

		public AboutViewModel(IMainThread mainThread,
								LibrariesService librariesService,
								IAnalyticsService analyticsService,
								DeepLinkingService deepLinkingService,
								GitTrendsStatisticsService gitTrendsStatisticsService) : base(analyticsService, mainThread)
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
			GitTrendsContributors = gitTrendsStatisticsService.Contributors.OrderByDescending(x => x.ContributionCount).ToList();
		}

		public long? Stars { get; }
		public long? Forks { get; }
		public long? Watchers { get; }

		public IReadOnlyList<Contributor> GitTrendsContributors { get; }
		public IReadOnlyList<NuGetPackageModel> InstalledLibraries { get; }

		[RelayCommand]
		public Task ViewOnGitHub()
		{
			if (_gitTrendsStatisticsService?.GitHubUri is null)
				return Task.CompletedTask;

			AnalyticsService.Track("View On GitHub Tapped");

			return _deepLinkingService.OpenBrowser(_gitTrendsStatisticsService.GitHubUri);
		}

		[RelayCommand]
		public Task RequestFeature()
		{
			if (_gitTrendsStatisticsService?.GitHubUri is null)
				return Task.CompletedTask;

			AnalyticsService.Track("Request Feature Tapped");

			return _deepLinkingService.OpenBrowser(_gitTrendsStatisticsService.GitHubUri + "/issues/new?template=feature_request.md");
		}
	}
}