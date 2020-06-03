using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;

namespace GitTrends.Functions
{
    class GitHubApiV3Service : BaseApiService
    {
        readonly IGitHubApiV3 _gitHubApiV3Client;

        public GitHubApiV3Service(IGitHubApiV3 gitHubApiV3Client) => _gitHubApiV3Client = gitHubApiV3Client;

        public Task<HttpResponseMessage> GetGitHubApiResponse(string token, CancellationToken cancellationToken) =>
            AttemptAndRetry(() => _gitHubApiV3Client.GetGitHubApiResponse_Authenticated($"Bearer {token}"), cancellationToken);
    }
}
