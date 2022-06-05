using System.Threading;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
	public class ConnectToGitHubOnboardingPage : BaseOnboardingContentPage
	{
		public ConnectToGitHubOnboardingPage(IDeviceInfo deviceInfo,
												IMainThread mainthread,
												IAnalyticsService analyticsService,
												MediaElementService mediaElementService)
				: base(OnboardingConstants.TryDemoText, deviceInfo, Color.FromHex(BaseTheme.CoralColorHex), mainthread, 3, analyticsService, mediaElementService)
		{
			GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
		}

		enum Row { Description, Button, ActivityIndicator }

		protected override View CreateImageView() => new Image
		{
			Source = "ConnectToGitHubOnboarding",
			Aspect = Aspect.AspectFit
		}.CenterExpand();

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
						.Invoke(button => button.CommandParameter = (CancellationToken.None, new Xamarin.Essentials.BrowserLaunchOptions
						{
							PreferredControlColor = Color.White,
							PreferredToolbarColor = Color.FromHex(BaseTheme.CoralColorHex).MultiplyAlpha(0.75),
							Flags = Xamarin.Essentials.BrowserLaunchFlags.PresentAsFormSheet,
						})),

					new IsAuthenticatingIndicator().Row(Row.ActivityIndicator)
				}
			}
		};

		void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e)
		{
			if (e.IsSessionAuthorized)
				MainThread.BeginInvokeOnMainThread(() => Navigation.PopModalAsync());
		}

		class IsAuthenticatingIndicator : ActivityIndicator
		{
			public IsAuthenticatingIndicator()
			{
				Color = Color.White;

				AutomationId = OnboardingAutomationIds.IsAuthenticatingActivityIndicator;

				this.SetBinding(IsVisibleProperty, nameof(GitHubAuthenticationViewModel.IsAuthenticating));
				this.SetBinding(IsRunningProperty, nameof(GitHubAuthenticationViewModel.IsAuthenticating));
			}
		}
	}
}