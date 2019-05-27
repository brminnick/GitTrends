using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class RepositoryResponse
    {
        public RepositoryResponse(Repository repository) => Repository = repository;

        [JsonProperty("repository")]
        public Repository Repository { get; }
    }

}
