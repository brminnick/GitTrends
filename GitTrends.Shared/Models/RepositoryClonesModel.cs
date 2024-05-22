using System.Text.Json.Serialization;

namespace GitTrends.Shared
{
	public record RepositoryClonesResponseModel : BaseRepositoryModel
	{
		public RepositoryClonesResponseModel(long count, long uniques, IEnumerable<DailyClonesModel> clones, string repositoryName = "", string repositoryOwner = "")
			: base(count, uniques, repositoryName, repositoryOwner)
		{
			DailyClonesList = clones.ToList();
		}

		[JsonPropertyName("clones")]
		public List<DailyClonesModel> DailyClonesList { get; }
	}
}