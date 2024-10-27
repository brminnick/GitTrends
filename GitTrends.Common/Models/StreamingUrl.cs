namespace GitTrends.Common;

public record StreamingManifest
{
	public StreamingManifest(string manifestUrl)
	{
		ManifestUrl = manifestUrl;
		HlsUrl = manifestUrl + "(format=m3u8-cmaf)";
		DashUrl = manifestUrl + "(format=mpd-time-cmaf)";
	}

	public string ManifestUrl { get; }
	public string HlsUrl { get; }
	public string DashUrl { get; }
}