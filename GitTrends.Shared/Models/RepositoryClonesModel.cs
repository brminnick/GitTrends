namespace GitTrends.Shared;

public record RepositoryClonesModel : RepositoryClonesResponseModel, IBaseRepositoryModel
{
	public RepositoryClonesModel(long totalCount, long totalUniqueCount, IEnumerable<DailyClonesModel> dailyClonesList, string repositoryName, string repositoryOwner)
		: base(totalCount, totalUniqueCount, dailyClonesList.ToList())
	{
		(RepositoryName, RepositoryOwner) = (repositoryName, repositoryOwner);
	}

	public string RepositoryName { get; }
	public string RepositoryOwner { get; }
}