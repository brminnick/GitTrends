using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.Neither)]
    [TestFixture(Platform.iOS, UserType.Neither)]
    class OnboardingTests : BaseUITest
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
            //Arrange
            bool areNotificationsEnabled_Initial = OnboardingPage.AreNotificationsEnabeld;
            bool areNotificationsEnabled_Final;

            //Assert
            Assert.AreEqual(0, OnboardingPage.CurrentPageNumber);
            Assert.AreEqual(OnboardingConstants.GitTrendsPage_Title, OnboardingPage.TitleLabelText);

            //Act
            await OnboardingPage.MoveToNextPage().ConfigureAwait(false);
            await OnboardingPage.WaitForPageToLoad().ConfigureAwait(false);

            //Assert
            Assert.AreEqual(1, OnboardingPage.CurrentPageNumber);
            Assert.AreEqual(OnboardingConstants.ChartPage_Title, OnboardingPage.TitleLabelText);

            //Act
            await OnboardingPage.MoveToNextPage().ConfigureAwait(false);
            await OnboardingPage.WaitForPageToLoad().ConfigureAwait(false);
            OnboardingPage.TapEnableNotificationsButton();

            //Assert
            areNotificationsEnabled_Final = OnboardingPage.AreNotificationsEnabeld;

            Assert.AreEqual(2, OnboardingPage.CurrentPageNumber);
            Assert.AreEqual(OnboardingConstants.NotificationsPage_Title, OnboardingPage.TitleLabelText);

            Assert.IsTrue(areNotificationsEnabled_Final);
            Assert.IsFalse(areNotificationsEnabled_Initial);

            //Act
            await OnboardingPage.MoveToNextPage().ConfigureAwait(false);
            await OnboardingPage.WaitForPageToLoad().ConfigureAwait(false);

            //Assert
            Assert.AreEqual(3, OnboardingPage.CurrentPageNumber);
            Assert.AreEqual(OnboardingConstants.ConnectToGitHubPage_Title, OnboardingPage.TitleLabelText);
        }
    }
}
