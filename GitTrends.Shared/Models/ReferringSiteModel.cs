using System;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class ReferringSiteModel : BaseRepositoryModel
    {
        public ReferringSiteModel(long count, long uniques, string referrer) : base(count, uniques)
        {
            Referrer = referrer;
            IsReferrerUriValid = Uri.TryCreate("https://" + referrer, UriKind.Absolute, out var referringUri);

            if (IsReferrerUriValid && !referringUri.ToString().Contains('.'))
                ReferrerUri = new Uri(referringUri.ToString().TrimEnd('/') + ".com/");
            else if (IsReferrerUriValid)
                ReferrerUri = referringUri;
            else
                ReferrerUri = null;
        }

        [JsonProperty("referrer")]
        public string Referrer { get; }

        [JsonProperty("isReferrerUriValid")]
        public bool IsReferrerUriValid { get; }

        [JsonProperty("referrerUri")]
        public Uri? ReferrerUri { get; }
    }
}
