using Newtonsoft.Json;

namespace GitTrends.Shared
{
    class GraphQLResponse<T>
    {
        public GraphQLResponse(T data, GraphQLError[] errors)
        {
            Data = data;
            Errors = errors;
        }

        [JsonProperty("data")]
        public T Data { get; }

        [JsonProperty("errors")]
        public GraphQLError[] Errors { get; }
    }
}
