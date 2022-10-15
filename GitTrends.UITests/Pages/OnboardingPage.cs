using System;
using System.Threading.Tasks;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Xamarin.UITest;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace GitTrends.UITests
{
	class OnboardingPage : BaseCarouselPage
	{
		readonly Query _connectToGitHubButton, _enableNotificationsButton, _nextButton,
			_pageIndicator, _titleLabel, _activityIndicator, _onboardingPage;

		public OnboardingPage(IApp app) : base(app, BackdoorMethodConstants.GetCurrentOnboardingPageNumber)
		{
			_activityIndicator = GenerateMarkedQuery(OnboardingAutomationIds.IsAuthenticatingActivityIndicator);
			_connectToGitHubButton = GenerateMarkedQuery(OnboardingAutomationIds.ConnectToGitHubButton);
			_enableNotificationsButton = GenerateMarkedQuery(OnboardingAutomationIds.EnableNotificationsButton);
			_onboardingPage = GenerateMarkedQuery(OnboardingAutomationIds.OnboardingPage);
			_nextButton = GenerateMarkedQuery(OnboardingAutomationIds.NextButon);
			_pageIndicator = GenerateMarkedQuery(OnboardingAutomationIds.PageIndicator);
			_titleLabel = GenerateMarkedQuery(OnboardingAutomationIds.TitleLabel);
		}

		public string TitleLabelText => GetText(_titleLabel);

		public bool AreNotificationsEnabeld => App.InvokeBackdoorMethod<bool>(BackdoorMethodConstants.ShouldSendNotifications);

		public override Task WaitForPageToLoad(TimeSpan? timeout = null)
		{
			App.WaitForElement(_onboardingPage, timeout: timeout);
			App.WaitForNoElement(_activityIndicator, timeout: timeout);

			switch (CurrentPageNumber)
			{
				case 0:
					App.WaitForElement(static x => x.Text(OnboardingConstants.GitTrendsPage_Title), timeout: timeout);
					break;
				case 1:
					App.WaitForElement(static x => x.Text(OnboardingConstants.ChartPage_Title), timeout: timeout);
					break;
				case 2:
					App.WaitForElement(static x => x.Text(OnboardingConstants.NotificationsPage_Title), timeout: timeout);
					break;
				case 3:
					App.WaitForElement(static x => x.Text(OnboardingConstants.ConnectToGitHubPage_Title), timeout: timeout);
					break;

				default:
					throw new NotSupportedException();
			}

			return Task.CompletedTask;
		}

		public void WaitForIsAuthenticatingActivityIndicator()
		{
			App.WaitForElement(_activityIndicator);
			App.Screenshot("Activity Indicator Appeared");
		}

		public void WaitForNoIsAuthenticatingActivityIndicator()
		{
			App.WaitForNoElement(_activityIndicator);
			App.Screenshot("Activity Indicator Disappeared");
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