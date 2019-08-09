using System.Collections.Generic;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class RepositoryClonesModel : BaseRepositoryModel
    {
        public RepositoryClonesModel(long count, long uniques, List<DailyClonesModel> clones) : base(count, uniques)
        {
            DailyClonesList = clones;
        }

        [JsonProperty("clones")]
        public List<DailyClonesModel> DailyClonesList { get; }
    }
}
