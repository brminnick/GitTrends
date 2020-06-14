using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitTrends.Shared
{
    public class GraphQLError
    {
        public GraphQLError(string message, GraphQLLocation[] locations) => (Message, Locations) = (message, locations);

        [JsonProperty("message")]
        public string Message { get; }

        [JsonProperty("locations")]
        public GraphQLLocation[] Locations { get; }

        [JsonExtensionData]
        public IDictionary<string, JToken>? AdditonalEntries { get; set; }
    }

    public class GraphQLLocation
    {
        [JsonProperty("line")]
        public long Line { get; }

        [JsonProperty("column")]
        public long Column { get; }
    }
}
