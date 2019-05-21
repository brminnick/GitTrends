using System;
using Newtonsoft.Json;
namespace GitTrends.Shared
{
    public class GenerateTokenDTO
    {
        [Obsolete("Use Overloaded Constructor")]
        public GenerateTokenDTO(){}

        [JsonConstructor]
        public GenerateTokenDTO(string loginCode, string state) => (LoginCode, State) = (loginCode, state);

        [JsonProperty("loginCode")]
        public string LoginCode { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}
