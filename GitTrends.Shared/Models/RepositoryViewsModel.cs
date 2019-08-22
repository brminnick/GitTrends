using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class RepositoryViewsModel : BaseRepositoryModel
    {
        public RepositoryViewsModel(long count, long uniques, IEnumerable<DailyViewsModel> views) : base(count, uniques)
        {
            DailyViewsList = views.ToList();
        }

        [JsonProperty("views")]
        public List<DailyViewsModel> DailyViewsList { get; }
    }
}