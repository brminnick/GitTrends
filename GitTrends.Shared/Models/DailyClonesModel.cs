using System;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class DailyClonesModel : BaseDailyModel, IDailyClonesModel
    {
        public DailyClonesModel(DateTimeOffset timestamp, long count, long uniques) : base(timestamp, count, uniques)
        {

        }

        [JsonIgnore]
        public long TotalClones => TotalCount;

        [JsonIgnore]
        public long TotalUniqueClones => TotalUniqueCount;
    }
}
