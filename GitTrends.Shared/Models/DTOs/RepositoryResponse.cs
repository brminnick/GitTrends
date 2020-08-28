using System;

namespace GitTrends.Shared
{
    public class RepositoryResponse
    {
        public RepositoryResponse(RepositoryResponseData repository) => Repository = repository;

        public RepositoryResponseData Repository { get; }
    }

    public class RepositoryResponseData
    {
        public RepositoryResponseData(string name, string description, long forkCount, Uri url, RepositoryOwner owner, bool isFork, IssuesConnection issues)
        {
            Name = name;
            Description = description;
            ForkCount = forkCount;
            Url = url;
            Owner = owner;
            IsFork = isFork;
            Issues = issues;
        }

        public string Name { get; }
        public string Description { get; }
        public long ForkCount { get; }
        public Uri Url { get; }
        public RepositoryOwner Owner { get; }
        public bool IsFork { get; }
        public IssuesConnection Issues { get; }
    }
}
