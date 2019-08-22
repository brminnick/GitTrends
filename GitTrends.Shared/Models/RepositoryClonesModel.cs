using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class RepositoryClonesModel : BaseRepositoryModel
    {
        public RepositoryClonesModel(long count, long uniques, IEnumerable<DailyClonesModel> clones) : base(count, uniques)
        {
            DailyClonesList = clones?.ToList() ?? Enumerable.Empty<DailyClonesModel>().ToList();
        }

        [JsonProperty("clones")]
        public List<DailyClonesModel> DailyClonesList { get; }
    }
}
