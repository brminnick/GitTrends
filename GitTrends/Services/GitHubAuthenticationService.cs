using System;
using System.Threading.Tasks;
using System.Web;
using Octokit;

namespace GitTrends
{
    public abstract class GitHubAuthenticationService : BaseGitHubService
    {
        static string _sessionId;
        const string _clientId = "cd7d3240c298193b55de";

        public static Task LaunchWebAuthenticationPage()
        {
            _sessionId = Guid.NewGuid().ToString();

            var request = new OauthLoginRequest(_clientId)
            {
                Scopes = { "repo" },
                State = _sessionId
            };

            var oauthLoginUrl = GitHubClient.Oauth.GetGitHubLoginUrl(request);

            return XamarinFormsServices.BeginInvokeOnMainThreadAsync(() => Xamarin.Essentials.Browser.OpenAsync(oauthLoginUrl));
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

            var request = new OauthTokenRequest(_clientId, _clientSecret, code);

            var token = await GitHubClient.Oauth.CreateAccessToken(request);

            await SetOAuthToken(token).ConfigureAwait(false);
        }
    }
}
