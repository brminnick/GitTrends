using System.Collections.Generic;
using System.Linq;

namespace GitTrends.Shared
{
    public static class RepositoryService
    {
        public static IEnumerable<Repository> RemoveForksAndDuplicates(in IEnumerable<Repository> repositoriesList) =>
              repositoriesList.Where(x => !x.IsFork).OrderByDescending(x => x.DataDownloadedAt).GroupBy(x => x.Name).Select(x => x.FirstOrDefault(x => x.DailyViewsList.Any()) ?? x.First());
    }
}
