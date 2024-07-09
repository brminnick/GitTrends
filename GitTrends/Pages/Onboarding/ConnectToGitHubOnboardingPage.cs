using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

public class ConnectToGitHubOnboardingPage : BaseOnboardingContentPage
{
	public ConnectToGitHubOnboardingPage(IDeviceInfo deviceInfo, IAnalyticsService analyticsService)
		: base(OnboardingConstants.TryDemoText, deviceInfo, Color.FromArgb(BaseTheme.CoralColorHex), 3, analyticsService)
	{
		GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
	}

	enum Row { Description, Button, ActivityIndicator }

	protected override View CreateImageView() => new Image
	{
		Source = "ConnectToGitHubOnboarding",
		Aspect = Aspect.AspectFit
	}.Center();

	protected override TitleLabel CreateDescriptionTitleLabel() => new TitleLabel(OnboardingConstants.ConnectToGitHubPage_Title);

	protected override View CreateDescriptionBodyView() => new ScrollView
	{
		Content = new Grid
		{
			RowSpacing = 16,

			RowDefinitions = Rows.Define(
				(Row.Description, 65),
				(Row.Button, 42),
				(Row.ActivityIndicator, 42)),

			Children =
			{
				new BodyLabel(OnboardingConstants.ConnectToGitHubPage_Body_GetStarted).Row(Row.Description),

				new GitHubButton(OnboardingAutomationIds.ConnectToGitHubButton, GitHubLoginButtonConstants.ConnectToGitHub)
					.Row(Row.Button)
					.Bind(GitHubButton.CommandProperty, nameof(OnboardingViewModel.HandleConnectToGitHubButtonCommand))
					.Invoke(button => button.CommandParameter = (CancellationToken.None, new BrowserLaunchOptions
					{
						PreferredControlColor = Colors.White,
						PreferredToolbarColor = Color.FromArgb(BaseTheme.CoralColorHex).MultiplyAlpha(0.75f),
						Flags = BrowserLaunchFlags.PresentAsFormSheet,
					})),

				new IsAuthenticatingIndicator().Row(Row.ActivityIndicator)
			}
		}
	};

	void HandleAuthorizeSessionCompleted(object? sender, AuthorizeSessionCompletedEventArgs e)
	{
		if (e.IsSessionAuthorized)
			Dispatcher.DispatchAsync(() => Navigation.PopModalAsync());
	}

	class IsAuthenticatingIndicator : ActivityIndicator
	{
		public IsAuthenticatingIndicator()
		{
			Color = Colors.White;

			AutomationId = OnboardingAutomationIds.IsAuthenticatingActivityIndicator;

			this.SetBinding(IsVisibleProperty, nameof(GitHubAuthenticationViewModel.IsAuthenticating));
			this.SetBinding(IsRunningProperty, nameof(GitHubAuthenticationViewModel.IsAuthenticating));
		}
	}
}