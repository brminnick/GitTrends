using System;
using System.Collections.Generic;
using System.Linq;

namespace GitTrends.Shared
{
	public static class RepositoryService
	{
		public static IEnumerable<Repository> RemoveForksAndDuplicates(this IEnumerable<Repository> repositoriesList, Func<Repository, bool> duplicateRepositoryPriorityFilter) =>
			repositoriesList.Where(x => !x.IsFork).OrderByDescending(x => x.DataDownloadedAt).GroupBy(x => x.Name).Select(x => x.FirstOrDefault(duplicateRepositoryPriorityFilter) ?? x.First());
	}
}