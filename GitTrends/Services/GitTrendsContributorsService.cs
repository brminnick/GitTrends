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
    public class GitTrendsContributorsService
    {
        readonly IPreferences _preferences;
        readonly IAnalyticsService _analyticsService;
        readonly GitHubApiV3Service _gitHubApiV3Service;

        public GitTrendsContributorsService(IPreferences preferences,
                                                IAnalyticsService analyticsService,
                                                GitHubApiV3Service gitHubApiV3Service)
        {
            _preferences = preferences;
            _analyticsService = analyticsService;
            _gitHubApiV3Service = gitHubApiV3Service;
        }

        public IReadOnlyList<Contributor> Contributors
        {
            get => GetContributors();
            private set => _preferences.Set(nameof(Contributors), JsonConvert.SerializeObject(value));
        }

        public async ValueTask Initialize(CancellationToken cancellationToken)
        {
            var dataDownloadedAt = Contributors.FirstOrDefault()?.DataDownloadedAt ?? default;

            if (Contributors.Any())
                initialize(cancellationToken).SafeFireAndForget();
            else
                await initialize(cancellationToken).ConfigureAwait(false);

            async Task initialize(CancellationToken cancellationToken)
            {
                try
                {
                    Contributors = await _gitHubApiV3Service.GetGitTrendsContributors(cancellationToken).ConfigureAwait(false);
                }
                catch(Exception e)
                {
                    _analyticsService.Report(e);
                }
            }
        }

        IReadOnlyList<Contributor> GetContributors()
        {
            try
            {
                var contributors = JsonConvert.DeserializeObject<IReadOnlyList<Contributor>>(_preferences.Get(nameof(Contributors), null));
                return contributors ?? new List<Contributor>();
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
