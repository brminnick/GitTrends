using System;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Mobile.Shared;
using NUnit.Framework;
using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
    class OnboardingPage : BasePage
    {
        readonly Query _connectToGitHubButton, _enableNotificationsButton, _nextButton,
            _pageIndicator, _titleLabel;

        public OnboardingPage(IApp app) : base(app)
        {
            _connectToGitHubButton = GenerateMarkedQuery(OnboardingAutomationIds.ConnectToGitHubButton);
            _enableNotificationsButton = GenerateMarkedQuery(OnboardingAutomationIds.EnableNotificationsButton);
            _nextButton = GenerateMarkedQuery(OnboardingAutomationIds.NextButon);
            _pageIndicator = GenerateMarkedQuery(OnboardingAutomationIds.PageIndicator);
            _titleLabel = GenerateMarkedQuery(OnboardingAutomationIds.TitleLabel);
        }

        public string TitleLabelText => GetText(_titleLabel);

        public int CurrentPageNumber => App.InvokeBackdoorMethod<int>(BackdoorMethodConstants.GetCurrentOnboardingPageNumber);

        public override Task WaitForPageToLoad(TimeSpan? timeout = null)
        {
            App.WaitForElement(_pageIndicator);

            switch (CurrentPageNumber)
            {
                case 0:
                    App.WaitForElement(OnboardingConstants.GitTrendsPageTitle);
                    break;
                case 1:
                    App.WaitForElement(OnboardingConstants.ChartPageTitle);
                    break;
                case 2:
                    App.WaitForElement(OnboardingConstants.NotificationsPageTitle);
                    break;
                case 3:
                    App.WaitForElement(OnboardingConstants.ConnectToGitHubPageTitle);
                    break;

                default:
                    throw new NotSupportedException();
            }

            return Task.CompletedTask;
        }

        public void MoveToNextPage()
        {
            var initialPageNumber = CurrentPageNumber;

            var screenSize = App.Query().First().Rect;
            App.DragCoordinates(screenSize.Width * 9 / 10, screenSize.CenterY, screenSize.Width * 1 / 10, screenSize.CenterY);

            var finalPageNumber = CurrentPageNumber;

            App.Screenshot($"Moved from Page {initialPageNumber} to Page {finalPageNumber}");

            Assert.GreaterOrEqual(finalPageNumber, initialPageNumber);
        }

        public void MoveToPreviousPage()
        {
            var initialPageNumber = CurrentPageNumber;

            var screenSize = App.Query().First().Rect;
            App.DragCoordinates(screenSize.Width * 1 / 10, screenSize.CenterY, screenSize.Width * 9 / 10, screenSize.CenterY);

            var finalPageNumber = CurrentPageNumber;

            App.Screenshot($"Moved from Page {initialPageNumber} to Page {finalPageNumber}");

            Assert.LessOrEqual(finalPageNumber, initialPageNumber);
        }

        public void TapConnectToGitHubButton()
        {
            App.Tap(_connectToGitHubButton);
            App.Screenshot("Tapped Connect To GitHub Button");
        }

        public void TapEnableNotificationsButton()
        {
            App.Tap(_enableNotificationsButton);
            App.Screenshot("Tapped Enable Notifications Button");
        }

        public void TapNextButton()
        {
            App.Tap(_nextButton);
            App.Screenshot("Tapped Next Button");
        }

        public void PopPage()
        {
            App.InvokeBackdoorMethod(BackdoorMethodConstants.PopPage);
            App.Screenshot("Onboarding Page Popped");
        }
    }
}
