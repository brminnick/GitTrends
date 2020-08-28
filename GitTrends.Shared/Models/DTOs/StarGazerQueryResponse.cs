using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class StarGazerResponse
    {
        public StarGazerResponse(RepositoryStarGazers repository) => Repository = repository;

        [JsonProperty("repository")]
        public RepositoryStarGazers Repository { get; }
    }

    public class RepositoryStarGazers
    {
        public RepositoryStarGazers(StarGazers starGazers) => StarGazers = starGazers;

        [JsonProperty("stargazers")]
        public StarGazers StarGazers { get; }
    }
}
