using System.Collections.Generic;
using System.Linq;

namespace GitTrends.Shared
{
    public static class RepositoryService
    {
        public static IEnumerable<Repository> RemoveForksAndDuplicates(in IEnumerable<Repository> repositoriesList) =>
            repositoriesList.Where(x => !x.IsFork).OrderByDescending(x => x.DataDownloadedAt).GroupBy(x => x.Name).Select(x => x.FirstOrDefault(x => DoesContainViewsClonesStarsData(x)) ?? x.First());

        static bool DoesContainViewsClonesStarsData(in Repository repository) => repository.TotalViews is not null
                                                                                    && repository.TotalUniqueViews is not null
                                                                                    && repository.TotalClones is not null
                                                                                    && repository.TotalUniqueClones is not null
                                                                                    && repository.StarCount is not null;
    }
}
