using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    class MockVersionTracking : IVersionTracking
    {
        public bool IsFirstLaunchEver => true;

        public bool IsFirstLaunchForCurrentVersion => true;

        public bool IsFirstLaunchForCurrentBuild => true;

        public string CurrentVersion => VersionHistory.Last();

        public string CurrentBuild => BuildHistory.Last();

        public string PreviousVersion => VersionHistory.Skip(1).Last();

        public string PreviousBuild => BuildHistory.Skip(1).Last();

        public string FirstInstalledVersion => PreviousVersion;

        public string FirstInstalledBuild => PreviousBuild;

        public IEnumerable<string> VersionHistory { get; } = new[] { "1.0.0", "1.0.1" };

        public IEnumerable<string> BuildHistory { get; } = new[] { "24", "25" };

        public bool IsFirstLaunchForBuild(string build) => true;

        public bool IsFirstLaunchForVersion(string version) => true;

        public void Track()
        {

        }
    }
}
