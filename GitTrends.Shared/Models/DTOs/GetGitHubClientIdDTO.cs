namespace GitTrends.Shared
{
    public class GetGitHubClientIdDTO
    {
        public GetGitHubClientIdDTO(string clientId) => ClientId = clientId;

        public string ClientId { get; }
    }
}
