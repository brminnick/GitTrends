using System;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    class DailyViewsModel : BaseDailyModel
    {
        public DailyViewsModel(DateTimeOffset day, long totalViews, long totalUniqueViews) :
            base(day, totalViews, totalUniqueViews)
        {

        }

        [JsonIgnore]
        public long TotalViews => TotalCount;

        [JsonIgnore]
        public long TotalUniqueViews => TotalUniqueCount;
    }
}
