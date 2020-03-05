using System;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.Neither)]
    [TestFixture(Platform.iOS, UserType.Neither)]
    class LaunchTests : BaseTest
    {
        public LaunchTests(Platform platform, UserType userType) : base(platform, userType)
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
                RepositoryPage.WaitForPageToLoad(TimeSpan.FromSeconds(10));
            }
        }
    }
}
