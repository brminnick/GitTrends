using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFImageLoading;
using GitTrends.Shared;

namespace GitTrends
{
    public class ImageCachingService
    {
        readonly IAnalyticsService _analyticsService;

        public ImageCachingService(IAnalyticsService analyticsService) => _analyticsService = analyticsService;

        public async Task PreloadRepositoryImages(IEnumerable<Repository> repositories)
        {
            foreach (var repository in getDistinctOwnerAvatarUrlList(repositories).Where(x => x.IsOwnerAvatarUrlValid()))
            {
                try
                {
                    await PreloadImage(repository.OwnerAvatarUrl).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _analyticsService.Report(e, new Dictionary<string, string>
                    {
                        { nameof(Repository) + nameof(Repository.Name), repository.Name },
                        { nameof(Repository) + nameof(Repository.OwnerLogin), repository.OwnerLogin },
                        { nameof(Repository) + nameof(Repository.OwnerAvatarUrl), repository.OwnerAvatarUrl }
                    });
                }
            }

            //Removes Duplicate OwnerAvatarUrls
            static IEnumerable<Repository> getDistinctOwnerAvatarUrlList(in IEnumerable<Repository> repositories) => repositories.GroupBy(x => x.OwnerAvatarUrl).Select(g => g.First());
        }

        public Task PreloadImage(in string url) => FFImageLoading.ImageService.Instance.LoadString(url).PreloadAsync();
    }
}
