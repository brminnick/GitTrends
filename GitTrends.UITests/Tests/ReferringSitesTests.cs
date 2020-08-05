using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;

namespace GitTrends.UITests
{
    [TestFixture(Platform.Android, UserType.Demo)]
    [TestFixture(Platform.Android, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.LoggedIn)]
    [TestFixture(Platform.iOS, UserType.Demo)]
    class ReferringSitesTests : BaseUITest
    {
        public ReferringSitesTests(Platform platform, UserType userType) : base(platform, userType)
        {
        }

        public override async Task BeforeEachTest()
        {
            await base.BeforeEachTest().ConfigureAwait(false);

            IReadOnlyList<ReferringSiteModel> referringSites = Enumerable.Empty<ReferringSiteModel>().ToList();

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

        [TestCase(ReviewAction.NoButtonTapped, ReviewAction.NoButtonTapped)]
        [TestCase(ReviewAction.NoButtonTapped, ReviewAction.YesButtonTapped)]
        [TestCase(ReviewAction.YesButtonTapped, ReviewAction.NoButtonTapped)]
        [TestCase(ReviewAction.YesButtonTapped, ReviewAction.YesButtonTapped)]
        public void VerifyStoreRequest(ReviewAction firstAction, ReviewAction secondAction)
        {
            //Arrange
            string firstTitleText, secondTitleText, firstNoButtonText, secondNoButtonText, firstYesButtonText, secondYesButtonText;

            //Act
            ReferringSitesPage.TriggerReviewRequest();
            ReferringSitesPage.WaitForReviewRequest();

            firstTitleText = ReferringSitesPage.StoreRatingRequestTitleLabelText;
            firstNoButtonText = ReferringSitesPage.StoreRatingRequestNoButtonText;
            firstYesButtonText = ReferringSitesPage.StoreRatingRequestYesButtonText;

            PerformReviewAction(firstAction);

            secondTitleText = ReferringSitesPage.StoreRatingRequestTitleLabelText;
            secondNoButtonText = ReferringSitesPage.StoreRatingRequestNoButtonText;
            secondYesButtonText = ReferringSitesPage.StoreRatingRequestYesButtonText;

            //Assert
            Assert.AreEqual(ReviewServiceConstants.TitleLabel_EnjoyingGitTrends, firstTitleText);
            Assert.AreEqual(ReviewServiceConstants.NoButton_NotReally, firstNoButtonText);
            Assert.AreEqual(ReviewServiceConstants.YesButton_Yes, firstYesButtonText);
            Assert.AreEqual(ReviewServiceConstants.NoButton_NoThanks, secondNoButtonText);
            Assert.AreEqual(ReviewServiceConstants.YesButton_OkSure, secondYesButtonText);

            if (firstAction is ReviewAction.NoButtonTapped)
                Assert.AreEqual(ReviewServiceConstants.TitleLabel_Feedback, secondTitleText);
            else
                Assert.AreEqual(ReferringSitesPage.ExpectedAppStoreRequestTitle, secondTitleText);

            //Act
            PerformReviewAction(secondAction);

            if (secondAction is ReviewAction.NoButtonTapped)
                ReferringSitesPage.WaitForNoReviewRequest();
        }

        void PerformReviewAction(in ReviewAction reviewAction)
        {
            if (reviewAction is ReviewAction.YesButtonTapped)
                ReferringSitesPage.TapStoreRatingRequestYesButton();
            else if (reviewAction is ReviewAction.NoButtonTapped)
                ReferringSitesPage.TapStoreRatingRequestNoButton();
            else
                throw new NotSupportedException();
        }
    }
}
