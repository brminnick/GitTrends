using System.Collections.Generic;
using System.Linq;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    class AboutViewModel : BaseViewModel
    {
        public AboutViewModel(IMainThread mainThread,
                                LibrariesService librariesService,
                                IAnalyticsService analyticsService,
                                GitTrendsStatisticsService gitTrendsStatisticsService) : base(analyticsService, mainThread)
        {
            if (gitTrendsStatisticsService.Stars.HasValue)
                Stars = gitTrendsStatisticsService.Stars.Value;

            if (gitTrendsStatisticsService.Watchers.HasValue)
                Watchers = gitTrendsStatisticsService.Watchers.Value;

            if (gitTrendsStatisticsService.Forks.HasValue)
                Forks = gitTrendsStatisticsService.Forks.Value;

            GitTrendsContributors = gitTrendsStatisticsService.Contributors.OrderByDescending(x => x.ContributionCount).ToList();
            InstalledLibraries = librariesService.InstalledLibraries;
        }

        public long Stars { get; }
        public long Forks { get; }
        public long Watchers { get; }
        public IReadOnlyList<Contributor> GitTrendsContributors { get; }
        public IReadOnlyList<NuGetPackageModel> InstalledLibraries { get; }
    }
}
