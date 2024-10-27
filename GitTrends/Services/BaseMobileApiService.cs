using GitTrends.Common;

namespace GitTrends;

public abstract class BaseMobileApiService(IAnalyticsService analyticsService)
{
	protected IAnalyticsService AnalyticsService { get; } = analyticsService;

	protected static string GetGitHubBearerTokenHeader(GitHubToken token) => token.IsEmpty() ? string.Empty : $"{token.TokenType} {token.AccessToken}";
}