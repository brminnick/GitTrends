using System;
using GitTrends.Shared;

namespace GitTrends.UITests
{
    public class ReferringSiteModel : IReferringSiteModel
    {
        public DateTimeOffset DownloadedAt { get; set; }
        public string Referrer { get; set; } = string.Empty;
        public bool IsReferrerUriValid { get; set; }
        public Uri? ReferrerUri { get; set; }
        public long TotalCount { get; set; }
        public long TotalUniqueCount { get; set; }
    }
}
