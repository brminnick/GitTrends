using System.Collections.Generic;
using System.Linq;

namespace GitTrends.Shared
{
    public static class RepositoryService
    {
        public static IEnumerable<Repository> RemoveForksAndDuplicates(in IEnumerable<Repository> repositoriesList) =>
            repositoriesList.Where(x => !x.IsFork).OrderByDescending(x => x.DataDownloadedAt).GroupBy(x => x.Name).Select(x => x.FirstOrDefault(x => DoesContainViewsClonesStarsData(x)) ?? x.First());

        static bool DoesContainViewsClonesStarsData(in Repository repository) => repository.TotalViews > 0 || repository.TotalUniqueViews > 0 || repository.TotalClones > 0 || repository.TotalUniqueClones > 0 || repository.StarCount > 0;
    }
}
