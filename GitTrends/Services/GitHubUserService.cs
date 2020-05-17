using System;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class GitHubUserService
    {
        const string _oauthTokenKey = "OAuthToken";

        readonly IPreferences _preferences;
        readonly ISecureStorage _secureStorage;

        public GitHubUserService(IPreferences preferences, ISecureStorage secureStorage) =>
            (_preferences, _secureStorage) = (preferences, secureStorage);

        public bool IsDemoUser => Alias is DemoDataConstants.Alias;

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Alias);

        public string Alias
        {
            get => _preferences.Get(nameof(Alias), string.Empty);
            set => _preferences.Set(nameof(Alias), value);
        }

        public string Name
        {
            get => _preferences.Get(nameof(Name), string.Empty);
            set => _preferences.Set(nameof(Name), value);
        }

        public string AvatarUrl
        {
            get => _preferences.Get(nameof(AvatarUrl), string.Empty);
            set => _preferences.Set(nameof(AvatarUrl), value);
        }


        public async Task<GitHubToken> GetGitHubToken()
        {
            var serializedToken = await _secureStorage.GetAsync(_oauthTokenKey).ConfigureAwait(false);

            try
            {
                var token = JsonConvert.DeserializeObject<GitHubToken?>(serializedToken);

                return token ?? GitHubToken.Empty;
            }
            catch (ArgumentNullException)
            {
                return GitHubToken.Empty;
            }
            catch (JsonReaderException)
            {
                return GitHubToken.Empty;
            }
        }

        public Task SaveGitHubToken(GitHubToken token)
        {
            if (token is null)
                throw new ArgumentNullException(nameof(token));

            if (token.AccessToken is null)
                throw new ArgumentNullException(nameof(token.AccessToken));

            var serializedToken = JsonConvert.SerializeObject(token);
            return _secureStorage.SetAsync(_oauthTokenKey, serializedToken);
        }

        public void InvalidateToken() => _secureStorage.Remove(_oauthTokenKey);
    }
}
