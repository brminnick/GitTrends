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
                                GitTrendsContributorsService gitTrendsContributorsService) : base(analyticsService, mainThread)
        {
            GitTrendsContributors = gitTrendsContributorsService.Contributors.OrderByDescending(x => x.ContributionCount).ToList();
            InstalledLibraries = librariesService.InstalledLibraries;
        }

        public IReadOnlyList<Contributor> GitTrendsContributors { get; }
        public IReadOnlyList<NuGetPackageModel> InstalledLibraries { get; }
    }
}
