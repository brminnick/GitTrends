using System;

namespace GitTrends.Shared
{
    interface IDailyClonesModel
    {
        public DateTime LocalDay { get; }

        public DateTimeOffset Day { get; }

        public long TotalClones { get; }

        public long TotalUniqueClones { get; }
    }
}
