namespace GitTrends.Shared
{
    public abstract record BaseRepositoryModel(long TotalCount, long TotalUniqueCount, string RepositoryName, string RepositoryOwner);
}
