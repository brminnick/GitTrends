using System;
namespace GitTrends
{
    public interface IMobileReferringSiteModel
    {
        public string FavIconImageUrl { get; }
        public DateTimeOffset DownloadedAt { get; }
        public string Referrer { get; }
        public bool IsReferrerUriValid { get; }
        public Uri? ReferrerUri { get; }
        public long TotalCount { get; }
        public long TotalUniqueCount { get; }
    }
}
