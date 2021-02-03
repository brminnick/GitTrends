using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    class AboutViewModel : BaseViewModel
    {
        public AboutViewModel(IMainThread mainThread,
                                LibrariesService librariesService,
                                IAnalyticsService analyticsService,
                                DeepLinkingService deepLinkingService,
                                GitTrendsStatisticsService gitTrendsStatisticsService) : base(analyticsService, mainThread)
        {
            if (gitTrendsStatisticsService.Stars.HasValue)
                Stars = gitTrendsStatisticsService.Stars.Value;

            if (gitTrendsStatisticsService.Watchers.HasValue)
                Watchers = gitTrendsStatisticsService.Watchers.Value;

            if (gitTrendsStatisticsService.Forks.HasValue)
                Forks = gitTrendsStatisticsService.Forks.Value;

            InstalledLibraries = librariesService.InstalledLibraries;
            GitTrendsContributors = gitTrendsStatisticsService.Contributors.OrderByDescending(x => x.ContributionCount).ToList();

            ViewOnGitHubCommand = new AsyncCommand(() =>
            {
                if (gitTrendsStatisticsService?.GitHubUri is null)
                    return Task.CompletedTask;

                return deepLinkingService.OpenBrowser(gitTrendsStatisticsService.GitHubUri);
            });

            RequestFeatureCommand = new AsyncCommand(() =>
            {
                if (gitTrendsStatisticsService?.GitHubUri is null)
                    return Task.CompletedTask;

                return deepLinkingService.OpenBrowser(gitTrendsStatisticsService.GitHubUri + "/issues/new?template=feature_request.md");
            });
        }

        public long? Stars { get; }
        public long? Forks { get; }
        public long? Watchers { get; }

        public IReadOnlyList<Contributor> GitTrendsContributors { get; }
        public IReadOnlyList<NuGetPackageModel> InstalledLibraries { get; }

        public IAsyncCommand ViewOnGitHubCommand { get; }
        public IAsyncCommand RequestFeatureCommand { get; }
    }
}
