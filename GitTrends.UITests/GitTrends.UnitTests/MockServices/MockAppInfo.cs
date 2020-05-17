using System;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
    public class MockAppInfo : IAppInfo
    {
        public string PackageName { get; } = "com.minnick.gittrends";

        public string Name { get; } = "GitTrends";

        public string VersionString { get; } = "1.1.1";

        public Version Version { get; } = new Version("1.1.1");

        public string BuildString { get; } = "23";

        public AppTheme RequestedTheme { get; } = AppTheme.Light;

        public void ShowSettingsUI()
        {

        }
    }
}
