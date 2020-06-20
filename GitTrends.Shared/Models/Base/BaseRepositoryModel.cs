namespace GitTrends.Shared
{
    public abstract class BaseRepositoryModel : BaseTotalCountModel
    {
        protected BaseRepositoryModel(long totalCount, long uniqueCount, string repositoryName, string repositoryOwner)
            : base(totalCount, uniqueCount)
        {
            RepositoryName = repositoryName;
            RepositoryOwner = repositoryOwner;
        }

        public string RepositoryName { get; }
        public string RepositoryOwner { get; }
    }
}
