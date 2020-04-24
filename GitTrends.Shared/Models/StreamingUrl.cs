namespace GitTrends.Shared
{
    public class StreamingUrl
    {
        public StreamingUrl(string manifestUrl) => (ManifestUrl, HlsUrl) = (manifestUrl, manifestUrl + "(format=m3u8-aapl)");

        public string ManifestUrl { get; }
        public string HlsUrl { get; }
    }
}
