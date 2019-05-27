using System;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class Issue
    {
        public Issue(string title, string body, DateTimeOffset createdAt, string state, DateTimeOffset? closedAt = null) =>
            (Title, Body, CreatedAt, ClosedAt, State) = (title, body, createdAt, closedAt, state);

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("body")]
        public string Body { get; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; }

        [JsonProperty("closedAt")]
        public DateTimeOffset? ClosedAt { get; }

        [JsonProperty("state")]
        public string State { get; }
    }
}
