namespace GitTrends.Shared
{
	public record StreamingManifest
	{
		public StreamingManifest(string manifestUrl)
		{
			ManifestUrl = manifestUrl;
			HlsUrl = manifestUrl + "(format=m3u8-aapl)";
			DashUrl = manifestUrl + "(format=mpd-time-csf)";
		}

		public string ManifestUrl { get; }
		public string HlsUrl { get; }
		public string DashUrl { get; }
	}
}