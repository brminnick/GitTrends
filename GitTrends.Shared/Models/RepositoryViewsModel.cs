using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public record RepositoryViewsResponseModel : BaseRepositoryModel
    {
        public RepositoryViewsResponseModel(long count, long uniques, IEnumerable<DailyViewsModel> views, string repositoryName = "", string repositoryOwner = "")
            : base(count, uniques, repositoryName, repositoryOwner)
        {
            DailyViewsList = views.ToList();
        }

        [JsonProperty("views")]
        public List<DailyViewsModel> DailyViewsList { get; }
    }
}