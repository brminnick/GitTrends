using GitHubApiStatus;
using GitTrends.Common;
namespace GitTrends.UnitTests;

public class ExtendedReferringSitesViewModel(
	IDispatcher mainThread,
	IAnalyticsService analyticsService,
	GitHubUserService gitHubUserService,
	ReferringSitesDatabase referringSitesDatabase,
	IGitHubApiStatusService gitHubApiStatusService,
	GitHubAuthenticationService gitHubAuthenticationService,
	GitHubApiRepositoriesService gitHubApiRepositoriesService)
	: ReferringSitesViewModel(mainThread, analyticsService, gitHubUserService, referringSitesDatabase, gitHubApiStatusService, gitHubAuthenticationService, gitHubApiRepositoriesService)
{
	public void SetRepository(Repository repository) => Repository = repository;
}