namespace GitTrends.Shared;

public record RepositoryViewsModel : RepositoryViewsResponseModel, IBaseRepositoryModel
{
	public RepositoryViewsModel(long totalCount, long totalUniqueCount, IEnumerable<DailyViewsModel> dailyViewsList, string repositoryName, string repositoryOwner)
		: base(totalCount, totalUniqueCount, dailyViewsList.ToList())
	{
		(RepositoryName, RepositoryOwner) = (repositoryName, repositoryOwner);
	}

	public string RepositoryName { get; }
	public string RepositoryOwner { get; }
}