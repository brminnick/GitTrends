using System.Threading.Tasks;
using GitTrends.Mobile.Common.Constants;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;

namespace GitTrends.UITests
{
	[TestFixture(Platform.iOS, UserType.Demo)]
	[TestFixture(Platform.iOS, UserType.LoggedIn)]
	[TestFixture(Platform.Android, UserType.Demo)]
	[TestFixture(Platform.Android, UserType.LoggedIn)]
	class AboutTests : BaseUITest
	{
		public AboutTests(Platform platform, UserType userType) : base(platform, userType)
		{
		}

		public override async Task BeforeEachTest()
		{
			await base.BeforeEachTest();

			RepositoryPage.TapSettingsButton();
			await SettingsPage.WaitForPageToLoad().ConfigureAwait(false);

			Assert.AreEqual(PageTitles.AboutPage, SettingsPage.AboutLabelText);

			SettingsPage.TapAboutButton();
			await AboutPage.WaitForPageToLoad().ConfigureAwait(false);
		}

		[Test]
		public void ViewOnGitHubButtonTest()
		{
			//Arrange

			//Act
			AboutPage.TapViewOnGitHubButton();

			//Assert
			if (App is iOSApp)
			{
				AboutPage.WaitForBrowserToOpen();
				Assert.IsTrue(AboutPage.IsBrowserOpen);
			}
		}

		[Test]
		public void RequestFeatureButtonTest()
		{
			//Arrange

			//Act
			AboutPage.TapRequestFeatureButton();

			//Assert
			if (App is iOSApp)
			{
				AboutPage.WaitForBrowserToOpen();
				Assert.IsTrue(AboutPage.IsBrowserOpen);
			}
		}

		[Test]
		public void VerifyStatisticsTest()
		{
			//Arrange

			//Act

			//Assert
			Assert.IsNotNull(AboutPage.ForkCount);
			Assert.IsNotNull(AboutPage.StarsCount);
			Assert.IsNotNull(AboutPage.WatchersCount);

			Assert.IsNotEmpty(AboutPage.Contributors);
			Assert.IsNotEmpty(AboutPage.InstalledLibraries);

			Assert.Greater(AboutPage.ForkCount, 0);
			Assert.Greater(AboutPage.StarsCount, 0);
			Assert.Greater(AboutPage.WatchersCount, 0);

			Assert.Greater(AboutPage.Contributors.Count, 0);
			Assert.Greater(AboutPage.InstalledLibraries.Count, 0);
		}
	}
}