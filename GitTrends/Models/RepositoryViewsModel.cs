using System.Collections.Generic;
using Newtonsoft.Json;

namespace GitTrends
{
    class RepositoryViewsModel : BaseRepositoryModel
    {
        public RepositoryViewsModel(long totalViewCount, long totalUniqueViewCount, List<DailyViewsModel> dailyViewsList) :
            base(totalViewCount, totalUniqueViewCount)
        {
            DailyViewsList = dailyViewsList;
        }

        [JsonProperty("views")]
        public List<DailyViewsModel> DailyViewsList { get; }
    }
}