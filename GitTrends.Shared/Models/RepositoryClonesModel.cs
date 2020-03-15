using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class RepositoryClonesResponseModel : BaseRepositoryModel
    {
        public RepositoryClonesResponseModel(string repositoryName, string repositoryOwner, long count, long uniques, IEnumerable<DailyClonesModel> clones) : base(repositoryName, repositoryOwner, count, uniques)
        {
            DailyClonesList = clones.ToList();
        }


        [JsonConstructor]
        public RepositoryClonesResponseModel(long count, long uniques, IEnumerable<DailyClonesModel> clones) : base(count, uniques)
        {
            DailyClonesList = clones.ToList();
        }

        [JsonProperty("clones")]
        public List<DailyClonesModel> DailyClonesList { get; }
    }
}
