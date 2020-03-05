using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    class ReplTests : BaseTest
    {
        public ReplTests(Platform platform) : base(platform, UserType.Neither)
        {
        }

        [Ignore("REPL used for manually exploring app")]
        [Test]
        public void ReplTest() => App.Repl();
    }
}
