using System;

namespace GitTrends.Shared
{
    public record ReferringSiteModel : BaseTotalCountModel, IReferringSiteModel
    {
        public ReferringSiteModel(in long count, in long uniques, in string referrer, in DateTimeOffset? downloadedAt = null) : base(count, uniques)
        {
            DownloadedAt = downloadedAt ?? DateTimeOffset.UtcNow;

            Referrer = referrer;
            Uri.TryCreate("https://" + referrer, UriKind.Absolute, out Uri? referringUri);

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
        public string Referrer { get; }
        public bool IsReferrerUriValid { get; }
        public Uri? ReferrerUri { get; }
    }
}
