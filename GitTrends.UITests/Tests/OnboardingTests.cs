using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.Neither)]
    [TestFixture(Platform.iOS, UserType.Neither)]
    class OnboardingTests : BaseTest
    {
        public OnboardingTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest().ConfigureAwait(false);

            await OnboardingPage.WaitForPageToLoad().ConfigureAwait(false);
        }

        [Test]
        public async Task EnsureEachPageLoads()
        {

            //Assert
            Assert.AreEqual(0, OnboardingPage.CurrentPageNumber);
            Assert.AreEqual(OnboardingConstants.GitTrendsPageTitle, OnboardingPage.TitleLabelText);

            //Act
            OnboardingPage.MoveToNextPage();
            await OnboardingPage.WaitForPageToLoad().ConfigureAwait(false);

            //Assert
            Assert.AreEqual(1, OnboardingPage.CurrentPageNumber);
            Assert.AreEqual(OnboardingConstants.ChartPageTitle, OnboardingPage.TitleLabelText);

            //Act
            OnboardingPage.MoveToNextPage();
            await OnboardingPage.WaitForPageToLoad().ConfigureAwait(false);

            //Assert
            Assert.AreEqual(2, OnboardingPage.CurrentPageNumber);
            Assert.AreEqual(OnboardingConstants.NotificationsPageTitle, OnboardingPage.TitleLabelText);

            //Act
            OnboardingPage.MoveToNextPage();
            await OnboardingPage.WaitForPageToLoad().ConfigureAwait(false);

            //Assert
            Assert.AreEqual(3, OnboardingPage.CurrentPageNumber);
            Assert.AreEqual(OnboardingConstants.ConnectToGitHubPageTitle, OnboardingPage.TitleLabelText);
        }
    }
}
