using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    class DailyViewsModel : BaseDailyModel
    {
        [JsonConstructor, Obsolete]
        public DailyViewsModel(DateTimeOffset timestamp, long count, long uniques, [CallerMemberName]string unused = null) : this(timestamp, count, uniques)
        {

        }

        public DailyViewsModel(DateTimeOffset day, long totalViews, long totalUniqueViews) : base(day, totalViews, totalUniqueViews)
        {

        }

        [JsonIgnore]
        public long TotalViews => TotalCount;

        [JsonIgnore]
        public long TotalUniqueViews => TotalUniqueCount;
    }
}
