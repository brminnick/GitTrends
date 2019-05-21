using System;
using Newtonsoft.Json;
namespace GitTrends.Shared
{
    public class GenerateTokenDTO
    {
        public GenerateTokenDTO(string loginCode, string state) => (LoginCode, State) = (loginCode, state);

        [JsonProperty("loginCode")]
        public string LoginCode { get; }

        [JsonProperty("state")]
        public string State { get; }
    }
}
