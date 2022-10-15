using System;
using System.Collections.Generic;
using System.Linq;

namespace GitTrends.Shared
{
	public static class RepositoryService
	{
		public static IEnumerable<Repository> RemoveForksDuplicatesAndArchives(this IEnumerable<Repository> repositoriesList, Func<Repository, bool>? duplicateRepositoryPriorityFilter = null)
		{
			duplicateRepositoryPriorityFilter ??= _ => true;

			return repositoriesList.Where(static x => !x.IsFork && !x.IsArchived).OrderByDescending(static x => x.DataDownloadedAt).GroupBy(static x => x.Name).Select(x => x.FirstOrDefault(duplicateRepositoryPriorityFilter) ?? x.First());
		}
	}
}