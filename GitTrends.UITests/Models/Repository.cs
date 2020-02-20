using System;
using GitTrends.Shared;

namespace GitTrends.UITests
{
    public class Repository : IRepository
    {
        public string OwnerLogin { get; set; } = string.Empty;

        public int StarCount { get; set; }

        public int IssuesCount { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public long ForkCount { get; set; }

#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
        public Uri? OwnerAvatarUrl { get; set; }

        public Uri? Uri { get; set; }
#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
    }
}
