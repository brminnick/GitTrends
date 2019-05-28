using Newtonsoft.Json;
namespace GitTrends.Shared
{
    public class GitHubViewerResponse
    {
        public GitHubViewerResponse(User viewer) => Viewer = viewer;

        [JsonProperty("viewer")]
        public User Viewer { get; }
    }
}
