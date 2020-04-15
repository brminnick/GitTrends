using System;
using System.Linq;
using Newtonsoft.Json;

namespace GitTrends.Shared
{
    public class ReferringSiteModel : BaseTotalCountModel
    {
        public ReferringSiteModel(long count, long uniques, string referrer) : base(count, uniques)
        {
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

        [JsonProperty("referrer")]
        public string Referrer { get; }

        [JsonProperty("isReferrerUriValid")]
        public bool IsReferrerUriValid { get; }

        [JsonProperty("referrerUri")]
        public Uri? ReferrerUri { get; }
    }
}
