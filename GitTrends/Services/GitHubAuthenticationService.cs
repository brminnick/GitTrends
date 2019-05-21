using System;
using System.Threading.Tasks;
using System.Web;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace GitTrends
{
    public static class GitHubAuthenticationService
    {
        const string _oauthTokenKey = "OAuthToken";

        static string _sessionId;

        public static async Task LaunchWebAuthenticationPage()
        {
            _sessionId = Guid.NewGuid().ToString();

            var clientId = await AzureFunctionsApiService.GetGitHubClientId().ConfigureAwait(false);
            clientId = clientId.Replace("\"", "");

            var githubUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&scope=repo&state={_sessionId}";

            await XamarinFormsServices.BeginInvokeOnMainThreadAsync(() => Browser.OpenAsync(githubUrl)).ConfigureAwait(false);
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

            var generateTokenDTO = new GenerateTokenDTO(code, state);
            var token = await AzureFunctionsApiService.GenerateGitTrendsOAuthToken(generateTokenDTO).ConfigureAwait(false);

            await SetAccessToken(token).ConfigureAwait(false);
        }

        public static async Task<GitHubToken> GetAccessToken()
        {
            var serializedToken = await SecureStorage.GetAsync(_oauthTokenKey).ConfigureAwait(false);

            return await Task.Run(() => JsonConvert.DeserializeObject<GitHubToken>(serializedToken)).ConfigureAwait(false);
        }

        static async Task SetAccessToken(GitHubToken token)
        {
            if (token is null)
                throw new ArgumentNullException(nameof(token));

            if (token?.AccessToken is null)
                throw new ArgumentNullException(nameof(token.AccessToken));

            var serializedToken = await Task.Run(() => JsonConvert.SerializeObject(token)).ConfigureAwait(false);
            await SecureStorage.SetAsync(_oauthTokenKey, serializedToken).ConfigureAwait(false);
        }
    }
}
