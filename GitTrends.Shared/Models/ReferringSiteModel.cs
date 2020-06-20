using System;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class ReferringSiteModel : BaseTotalCountModel
    {
        public ReferringSiteModel(in long count, in long uniques, in string referrer, in DateTimeOffset? downloadedAt = null) : base(count, uniques)
        {
            DownloadedAt = downloadedAt ?? DateTimeOffset.UtcNow;

            Referrer = referrer;
            Uri.TryCreate("https://" + referrer, UriKind.Absolute, out var referringUri);

            if (referringUri is null)
            {
                ReferrerUri = null;
                IsReferrerUriValid = false;
            }
            else if (!referringUri.ToString().Contains("."))
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

        public DateTimeOffset DownloadedAt { get; }

        [JsonProperty("referrer")]
        public string Referrer { get; }

        [JsonProperty("isReferrerUriValid")]
        public bool IsReferrerUriValid { get; }

        [JsonProperty("referrerUri")]
        public Uri? ReferrerUri { get; }
    }
}
