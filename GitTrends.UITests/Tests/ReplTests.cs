using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.Neither)]
    [TestFixture(Platform.iOS, UserType.Neither)]
    class ReplTests : BaseTest
    {
        public ReplTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        [Ignore("REPL used for manually exploring app")]
        [Test]
        public void ReplTest() => App.Repl();
    }
}
