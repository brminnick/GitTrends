using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    class RepositoryClonesModel : BaseRepositoryModel
    {
        [JsonConstructor, Obsolete]
        public RepositoryClonesModel(long count, long uniques, List<DailyClonesModel> clones, [CallerMemberName]string unused = null) : this(count, uniques, clones)
        {

        }

        public RepositoryClonesModel(long totalCloneCount, long totalUniqueCloneCount, List<DailyClonesModel> dailyClonesList) : base(totalCloneCount, totalUniqueCloneCount)
        {
            DailyClonesList = dailyClonesList;
        }

        [JsonProperty("clones")]
        public List<DailyClonesModel> DailyClonesList { get; }
    }
}
