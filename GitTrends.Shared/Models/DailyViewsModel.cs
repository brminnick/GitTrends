using System;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class DailyViewsModel : BaseDailyModel, IDailyViewsModel
    {
        public DailyViewsModel(DateTimeOffset timestamp, long count, long uniques) : base(timestamp, count, uniques)
        {

        }

        [JsonIgnore]
        public long TotalViews => TotalCount;

        [JsonIgnore]
        public long TotalUniqueViews => TotalUniqueCount;
    }
}
