using System.Collections.Generic;
using System.Linq;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel(IMainThread mainThread,
                                IAnalyticsService analyticsService,
                                GitTrendsContributorsService gitTrendsContributorsService) : base(analyticsService, mainThread)
        {
            GitTrendsContributors = gitTrendsContributorsService.Contributors.OrderByDescending(x => x.ContributionCount).ToList();
        }

        public IReadOnlyList<Contributor> GitTrendsContributors { get; }
    }
}
