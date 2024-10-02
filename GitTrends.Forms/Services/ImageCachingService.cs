using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFImageLoading;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public class ImageCachingService
	{
		readonly IMainThread _mainThread;
		readonly IAnalyticsService _analyticsService;

		public ImageCachingService(IMainThread mainThread, IAnalyticsService analyticsService) =>
			(_mainThread, _analyticsService) = (mainThread, analyticsService);

		public async Task PreloadRepositoryImages(IEnumerable<Repository> repositories)
		{
			foreach (var repository in getDistinctOwnerAvatarUrlList(repositories).Where(static x => x.IsOwnerAvatarUrlValid()))
			{
				await PreloadImageRepositoryImage(repository).ConfigureAwait(false);
			}

			//Removes Duplicate OwnerAvatarUrls
			static IEnumerable<Repository> getDistinctOwnerAvatarUrlList(in IEnumerable<Repository> repositories) => repositories.GroupBy(static x => x.OwnerAvatarUrl).Select(g => g.First());
		}

		public async Task PreloadImageRepositoryImage(Repository repository)
		{
			try
			{
				await PreloadImage(repository.OwnerAvatarUrl).ConfigureAwait(false);
			}
			catch (FFImageLoading.Exceptions.DownloadAggregateException e) when (e.Message.Contains("Could not connect to the server", StringComparison.OrdinalIgnoreCase))
			{

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

		// Ensure LoadUrl is always called from same thread: https://github.com/luberda-molinet/FFImageLoading/issues/1492#issuecomment-720711924
		public Task PreloadImage(string url) => _mainThread.InvokeOnMainThreadAsync(() => ImageService.Instance.LoadUrl(url).PreloadAsync());
		public Task PreloadImage(in Uri uri) => PreloadImage(uri.ToString());
	}
}