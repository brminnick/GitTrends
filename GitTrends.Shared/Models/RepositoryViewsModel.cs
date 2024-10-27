namespace  GitTrends.Common;

public record RepositoryViewsModel : RepositoryViewsResponseModel, IBaseRepositoryModel
{
	public RepositoryViewsModel(long totalCount, long totalUniqueCount, IEnumerable<DailyViewsModel> dailyViewsList, string repositoryName, string repositoryOwner)
		: base(totalCount, totalUniqueCount, [.. dailyViewsList])
	{
		(RepositoryName, RepositoryOwner) = (repositoryName, repositoryOwner);
	}

	public string RepositoryName { get; }
	public string RepositoryOwner { get; }
}