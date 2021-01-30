using System.Collections.Generic;
using System.Linq;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    class AboutViewModel : BaseViewModel
    {
        public AboutViewModel(IMainThread mainThread,
                                NuGetService nuGetService,
                                IAnalyticsService analyticsService,
                                GitTrendsContributorsService gitTrendsContributorsService) : base(analyticsService, mainThread)
        {
            GitTrendsContributors = gitTrendsContributorsService.Contributors.OrderByDescending(x => x.ContributionCount).ToList();
            NuGetPackageModels = nuGetService.InstalledNugetPackages;
        }

        public IReadOnlyList<Contributor> GitTrendsContributors { get; }
        public IReadOnlyList<NuGetPackageModel> NuGetPackageModels { get; }
    }
}
