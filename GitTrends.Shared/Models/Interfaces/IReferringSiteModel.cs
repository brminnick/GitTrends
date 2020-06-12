using System;
namespace GitTrends.Shared
{
    public interface IReferringSiteModel
    {
        public DateTimeOffset DownloadedAt { get; }
        public string Referrer { get; }
        public bool IsReferrerUriValid { get; }
        public Uri? ReferrerUri { get; }
        public long TotalCount { get; }
        public long TotalUniqueCount { get; }
    }
}
