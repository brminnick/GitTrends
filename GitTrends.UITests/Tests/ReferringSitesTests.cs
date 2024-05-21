using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;

namespace GitTrends.UITests
{
	[TestFixture(Platform.Android, UserType.Demo)]
	[TestFixture(Platform.Android, UserType.LoggedIn)]
	[TestFixture(Platform.iOS, UserType.Demo)]
	[TestFixture(Platform.iOS, UserType.LoggedIn)]
	class ReferringSitesTests : BaseUITest
	{
		public ReferringSitesTests(Platform platform, UserType userType) : base(platform, userType)
		{
		}

		public override async Task BeforeEachTest()
		{
			await base.BeforeEachTest().ConfigureAwait(false);

			IReadOnlyList<ReferringSiteModel> referringSites = [];

			var repositories = RepositoryPage.VisibleCollection;
			var repositoriesEnumerator = repositories.GetEnumerator();

			while (!referringSites.Any())
			{
				repositoriesEnumerator.MoveNext();
				RepositoryPage.TapRepository(repositoriesEnumerator.Current.Name);

				await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);
				TrendsPage.TapReferringSitesButton();

				await ReferringSitesPage.WaitForPageToLoad().ConfigureAwait(false);

				referringSites = ReferringSitesPage.VisibleCollection;

				if (!referringSites.Any())
				{
					Assert.IsTrue(ReferringSitesPage.IsEmptyDataViewVisible);

					ReferringSitesPage.ClosePage();

					await TrendsPage.WaitForPageToLoad().ConfigureAwait(false);
					TrendsPage.TapBackButton();

					await RepositoryPage.WaitForPageToLoad().ConfigureAwait(false);
				}
			}
		}

		[Test]
		public async Task ReferringSitesPageDoesLoad()
		{
			//Arrange
			IReadOnlyCollection<ReferringSiteModel> referringSiteList = ReferringSitesPage.VisibleCollection;
			var referringSite = referringSiteList.First();
			bool isUrlValid = referringSite.IsReferrerUriValid;

			//Assert
			Assert.IsTrue(App.Query(referringSite.Referrer).Any());

			//Act
			if (isUrlValid)
			{
				App.Tap(referringSite.Referrer);
				await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
			}

			//Assert
			if (isUrlValid && App is iOSApp)
			{
				SettingsPage.WaitForBrowserToOpen();
				Assert.IsTrue(ReferringSitesPage.IsBrowserOpen);
			}
		}
	}
}