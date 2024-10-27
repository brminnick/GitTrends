
namespace GitTrends.Common;

public class GetChartVideoDTO
{
	public GetChartVideoDTO(string url) => Url = url;

	public string Url { get; }
}