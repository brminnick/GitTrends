using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class ReferingSiteModel : BaseRepositoryModel
    {
        public ReferingSiteModel(long count, long uniques, string referrer) : base(count, uniques)
        {
            Referrer = referrer;
        }

        [JsonProperty("referrer")]
        public string Referrer { get; set; }
    }
}
