namespace GitTrends.Shared
{
    public class StreamingManifest
    {
        public StreamingManifest(string manifestUrl) => (ManifestUrl, HlsUrl) = (manifestUrl, manifestUrl + "(format=m3u8-aapl)");

        public string ManifestUrl { get; }
        public string HlsUrl { get; }
    }
}
