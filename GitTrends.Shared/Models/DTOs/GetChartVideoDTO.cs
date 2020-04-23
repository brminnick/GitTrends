
namespace GitTrends.Shared
{
    public class GetChartVideoDTO
    {
        public GetChartVideoDTO(string url) => Url = url;

        public string Url { get; }
    }
}
