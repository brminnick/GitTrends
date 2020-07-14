using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class GitHubUserService
    {
        const string _oauthTokenKey = "OAuthToken";

        readonly static WeakEventManager<string> _nameChangedEventManager = new WeakEventManager<string>();
        readonly static WeakEventManager<string> _aliasChangedEventManager = new WeakEventManager<string>();
        readonly static WeakEventManager<string> _avatarUrlChangedEventManager = new WeakEventManager<string>();

        readonly IPreferences _preferences;
        readonly ISecureStorage _secureStorage;

        public GitHubUserService(IPreferences preferences, ISecureStorage secureStorage) =>
            (_preferences, _secureStorage) = (preferences, secureStorage);

        public static event EventHandler<string> NameChanged
        {
            add => _nameChangedEventManager.AddEventHandler(value);
            remove => _nameChangedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<string> AliasChanged
        {
            add => _aliasChangedEventManager.AddEventHandler(value);
            remove => _aliasChangedEventManager.RemoveEventHandler(value);
        }

        public static event EventHandler<string> AvatarUrlChanged
        {
            add => _avatarUrlChangedEventManager.AddEventHandler(value);
            remove => _avatarUrlChangedEventManager.RemoveEventHandler(value);
        }

        public bool IsDemoUser => AvatarUrl == BaseTheme.GetGitTrendsImageSource();

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Alias);

        public string Alias
        {
            get => _preferences.Get(nameof(Alias), string.Empty);
            set
            {
                if (Alias != value)
                {
                    _preferences.Set(nameof(Alias), value);
                    OnAliasChanged(value);
                }
            }
        }

        public string Name
        {
            get => _preferences.Get(nameof(Name), string.Empty);
            set
            {
                if (Name != value)
                {
                    _preferences.Set(nameof(Name), value);
                    OnNameChanged(value);
                }
            }
        }

        public string AvatarUrl
        {
            get => _preferences.Get(nameof(AvatarUrl), string.Empty);
            set
            {
                if (AvatarUrl != value)
                {
                    _preferences.Set(nameof(AvatarUrl), value);
                    OnAvatarUrlChanged(value);
                }
            }
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

        void OnNameChanged(in string name) => _nameChangedEventManager.RaiseEvent(this, name, nameof(NameChanged));
        void OnAliasChanged(in string alias) => _aliasChangedEventManager.RaiseEvent(this, alias, nameof(AliasChanged));
        void OnAvatarUrlChanged(in string avatarUrl) => _avatarUrlChangedEventManager.RaiseEvent(this, avatarUrl, nameof(AvatarUrlChanged));
    }
}
