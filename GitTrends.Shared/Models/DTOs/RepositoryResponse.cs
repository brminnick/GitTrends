using System;

namespace GitTrends.Shared
{
    public record RepositoryResponse(RepositoryResponseData Repository);

    public record RepositoryResponseData(string Name,
                                            string Description,
                                            long ForkCount,
                                            Uri Url,
                                            RepositoryOwner Owner,
                                            bool IsFork,
                                            IssuesConnection Issues);
}
