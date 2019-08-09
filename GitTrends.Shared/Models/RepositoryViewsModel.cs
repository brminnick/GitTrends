using System.Collections.Generic;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class RepositoryViewsModel : BaseRepositoryModel
    {
        public RepositoryViewsModel(long count, long uniques, List<DailyViewsModel> views) : base(count, uniques)
        {
            DailyViewsList = views;
        }

        [JsonProperty("views")]
        public List<DailyViewsModel> DailyViewsList { get; }
    }
}