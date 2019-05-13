using System.Collections.Generic;
using Newtonsoft.Json;

namespace GitTrends
{
    class RepositoryClonesModel : BaseRepositoryModel
    {
        public RepositoryClonesModel(long totalCloneCount, long totalUniqueCloneCount, List<DailyClonesModel> dailyClonesList) :
            base(totalCloneCount, totalUniqueCloneCount)
        {
            DailyClonesList = dailyClonesList;
        }

        [JsonProperty("clones")]
        public List<DailyClonesModel> DailyClonesList { get; }
    }
}
