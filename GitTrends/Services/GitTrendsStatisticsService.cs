using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Shared;
using Newtonsoft.Json;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class GitTrendsStatisticsService
    {
        readonly IPreferences _preferences;
        readonly IAnalyticsService _analyticsService;
        readonly ImageCachingService _imageCachingService;
        readonly AzureFunctionsApiService _azureFunctionsApiService;

        public GitTrendsStatisticsService(IPreferences preferences,
                                            IAnalyticsService analyticsService,
                                            ImageCachingService imageCachingService,
                                            AzureFunctionsApiService azureFunctionsApiService)
        {
            _preferences = preferences;
            _analyticsService = analyticsService;
            _imageCachingService = imageCachingService;
            _azureFunctionsApiService = azureFunctionsApiService;
        }

        public Uri? EnableOrganizationsUri
        {
            get
            {
                var url = _preferences.Get(nameof(EnableOrganizationsUri), null);
                return url is null ? null : new Uri(url);
            }
            private set
            {
                if (value is not null)
                    _preferences.Set(nameof(EnableOrganizationsUri), value.ToString());
            }
        }

        public string? ClientId
        {
            get => _preferences.Get(nameof(ClientId), null);
            private set
            {
                if (value is not null)
                    _preferences.Set(nameof(ClientId), value.ToString());
            }
        }

        public Uri? GitHubUri
        {
            get
            {
                var url = _preferences.Get(nameof(GitHubUri), null);
                return url is null ? null : new Uri(url);
            }
            private set
            {
                if (value is not null)
                    _preferences.Set(nameof(GitHubUri), value.ToString());
            }
        }

        public long? Stars
        {
            get
            {
                var stars = _preferences.Get(nameof(Stars), (long)-1);
                return stars < 0 ? null : stars;
            }
            private set
            {
                if (value.HasValue)
                    _preferences.Set(nameof(Stars), value.Value);
            }
        }

        public long? Watchers
        {
            get
            {
                var watchers = _preferences.Get(nameof(Watchers), (long)-1);
                return watchers < 0 ? null : watchers;
            }
            private set
            {
                if (value.HasValue)
                    _preferences.Set(nameof(Watchers), value.Value);
            }
        }

        public long? Forks
        {
            get
            {
                var forks = _preferences.Get(nameof(Forks), (long)-1);
                return forks < 0 ? null : forks;
            }
            private set
            {
                if (value.HasValue)
                    _preferences.Set(nameof(Forks), value.Value);
            }
        }

        public IReadOnlyList<Contributor> Contributors
        {
            get => GetContributors();
            private set => _preferences.Set(nameof(Contributors), JsonConvert.SerializeObject(value));
        }

        public async ValueTask Initialize(CancellationToken cancellationToken)
        {
            if (EnableOrganizationsUri is null || ClientId is null || GitHubUri is null || Stars is null || Watchers is null || Forks is null || !Contributors.Any())
                await initialize().ConfigureAwait(false);
            else
                initialize().SafeFireAndForget(ex => _analyticsService.Report(ex));

            async Task initialize()
            {
                var getClientIdTask = _azureFunctionsApiService.GetGitHubClientId(cancellationToken);
                var getGitTrendsStatisticsTask = _azureFunctionsApiService.GetGitTrendsStatistics(cancellationToken);
                var getGitTrendsEnableOrganizationsUriTask = _azureFunctionsApiService.GetGitTrendsEnableOrganizationsUri(cancellationToken);

                await Task.WhenAll(getClientIdTask, getGitTrendsStatisticsTask, getGitTrendsEnableOrganizationsUriTask).ConfigureAwait(false);

                var clientId = await getClientIdTask.ConfigureAwait(false);
                var gittrendsStatistics = await getGitTrendsStatisticsTask.ConfigureAwait(false);
                var gitTrendsEnableOrganizationsUri = await getGitTrendsEnableOrganizationsUriTask.ConfigureAwait(false);

                Stars = gittrendsStatistics.Stars;
                Forks = gittrendsStatistics.Forks;
                Watchers = gittrendsStatistics.Watchers;
                GitHubUri = gittrendsStatistics.GitHubUri;

                Contributors = gittrendsStatistics.Contributors;

                ClientId = clientId.ClientId;

                EnableOrganizationsUri = gitTrendsEnableOrganizationsUri.Uri;

                foreach (var contributor in Contributors)
                    _imageCachingService.PreloadImage(contributor.AvatarUrl).SafeFireAndForget(ex => _analyticsService.Report(ex));
            }
        }

        IReadOnlyList<Contributor> GetContributors()
        {
            try
            {
                var contributors = JsonConvert.DeserializeObject<IReadOnlyList<Contributor>>(_preferences.Get(nameof(Contributors), null));
                return contributors ?? Array.Empty<Contributor>();
            }
            catch (ArgumentNullException)
            {
                return Array.Empty<Contributor>();
            }
            catch (JsonReaderException)
            {
                return Array.Empty<Contributor>();
            }
        }
    }
}
