using System;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    class LaunchTests : BaseTest
    {
        public LaunchTests(Platform platform) : base(platform)
        {
        
        }

        [Test]
        public void LaunchTest()
        {
            try
            {
                SplashScreenPage.WaitForPageToLoad(TimeSpan.FromSeconds(1));
            }
            catch
            {
                RepositoryPage.WaitForPageToLoad();
            }
        }
    }
}
