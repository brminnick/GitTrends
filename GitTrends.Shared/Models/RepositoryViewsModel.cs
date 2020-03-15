using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class RepositoryViewsResponseModel : BaseRepositoryModel
    {
        public RepositoryViewsResponseModel(string repositoryName, string repositoryOwner, long count, long uniques, IEnumerable<DailyViewsModel> views) : base(repositoryName, repositoryOwner, count, uniques)
        {
            DailyViewsList = views.ToList();
        }

        [JsonConstructor]
        public RepositoryViewsResponseModel(long count, long uniques, IEnumerable<DailyViewsModel> views) : base(count, uniques)
        {
            DailyViewsList = views.ToList();
        }

        [JsonProperty("views")]
        public List<DailyViewsModel> DailyViewsList { get; }
    }
}