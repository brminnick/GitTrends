using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class RepositoryClonesResponseModel : BaseRepositoryModel
    {
        public RepositoryClonesResponseModel(long count, long uniques, IEnumerable<DailyClonesModel> clones, string repositoryName = "", string repositoryOwner = "")
            : base(count, uniques, repositoryName, repositoryOwner)
        {
            DailyClonesList = clones.ToList();
        }

        [JsonProperty("clones")]
        public List<DailyClonesModel> DailyClonesList { get; }
    }
}
