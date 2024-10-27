using System.Text.Json.Serialization;
namespace GitTrends.Common;

public record ReferringSiteModel : BaseTotalCountModel, IReferringSiteModel
{
	public ReferringSiteModel(long count, long uniques, string referrer) : base(count, uniques)
	{
		Referrer = referrer;

		if (!Uri.TryCreate("https://" + referrer, UriKind.Absolute, out var referringUri))
		{
			ReferrerUri = null;
			IsReferrerUriValid = false;
		}
		else if (!referringUri.ToString().Contains('.'))
		{
			ReferrerUri = new Uri(referringUri.ToString().TrimEnd('/') + ".com/");
			IsReferrerUriValid = true;
		}
		else
		{
			ReferrerUri = referringUri;
			IsReferrerUriValid = true;
		}
	}

	public DateTimeOffset DownloadedAt { get; init; } = DateTimeOffset.UtcNow;

	[JsonPropertyName("referrer")]
	public string Referrer { get; }
	public bool IsReferrerUriValid { get; }
	public Uri? ReferrerUri { get; }
}