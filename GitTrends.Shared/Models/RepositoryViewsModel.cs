using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    class RepositoryViewsModel : BaseRepositoryModel
    {
        [JsonConstructor, Obsolete]
        public RepositoryViewsModel(long count, long uniques, List<DailyViewsModel> views, [CallerMemberName]string unused = null) : this(count, uniques, views)
        {

        }

        public RepositoryViewsModel(long totalViewCount, long totalUniqueViewCount, List<DailyViewsModel> dailyViewsList) : base(totalViewCount, totalUniqueViewCount)
        {
            DailyViewsList = dailyViewsList;
        }

        [JsonProperty("views")]
        public List<DailyViewsModel> DailyViewsList { get; }
    }
}