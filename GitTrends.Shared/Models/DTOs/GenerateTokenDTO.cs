using System;
using Newtonsoft.Json;
namespace GitTrends.Shared
{
    public class GenerateTokenDTO
    {
        public GenerateTokenDTO(string loginCode, string state) => (LoginCode, State) = (loginCode, state);

        public string LoginCode { get; }
        public string State { get; }
    }
}
