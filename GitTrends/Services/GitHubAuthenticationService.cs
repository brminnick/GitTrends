using System;
using System.Threading.Tasks;
using System.Web;
using Octokit;

namespace GitTrends
{
    public abstract class GitHubAuthenticationService : BaseGitHubService
    {
        static string _sessionId;

        public static async Task LaunchWebAuthenticationPage()
        {
            _sessionId = Guid.NewGuid().ToString();

            var clientId = await AzureFunctionsApiService.GetGitHubClientId().ConfigureAwait(false);

            var request = new OauthLoginRequest(clientId)
            {
                Scopes = { "repo" },
                State = _sessionId
            };

            var githubClient = await GetGitHubClient().ConfigureAwait(false);
            var oauthLoginUrl = githubClient.Oauth.GetGitHubLoginUrl(request);

            await XamarinFormsServices.BeginInvokeOnMainThreadAsync(() => Xamarin.Essentials.Browser.OpenAsync(oauthLoginUrl)).ConfigureAwait(false);
        }

        public static async Task AuthorizeSession(Uri callbackUri)
        {
            var code = HttpUtility.ParseQueryString(callbackUri.Query).Get("code");
            var state = HttpUtility.ParseQueryString(callbackUri.Query).Get("state");

            if (string.IsNullOrEmpty(code))
                throw new Exception("Invalid Authorization Code");

            if (state != _sessionId) 
                throw new InvalidOperationException("Invalid SessionId");

            _sessionId = string.Empty;

            var token = await AzureFunctionsApiService.GenerateGitTrendsOAuthToken(code).ConfigureAwait(false);

            await SetAccessToken(token).ConfigureAwait(false);
        }
    }
}
