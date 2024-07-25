﻿using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

public class ConnectToGitHubOnboardingView : BaseOnboardingContentView
{
	readonly IDispatcher _dispatcher;
	
	public ConnectToGitHubOnboardingView(IDeviceInfo deviceInfo, IDispatcher dispatcher, IAnalyticsService analyticsService)
		: base(
			OnboardingConstants.TryDemoText, 
			deviceInfo, 
			Color.FromArgb(BaseTheme.CoralColorHex), 
			3,
			() => new ImageView(),
			() => new TitleLabel(OnboardingConstants.ConnectToGitHubPage_Title),
			() => new DescriptionBodyView(),
			analyticsService)
	{
		_dispatcher = dispatcher;
		GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
	}

	enum Row { Description, Button, ActivityIndicator }
	
	async void HandleAuthorizeSessionCompleted(object? sender, AuthorizeSessionCompletedEventArgs e)
	{
		if (e.IsSessionAuthorized)
			await _dispatcher.DispatchAsync(() => Shell.Current.GoToAsync(".."));
	}

	sealed class ImageView : Image
	{
		public ImageView()
		{
			Source = "ConnectToGitHubOnboarding";
			Aspect = Aspect.AspectFit;
			this.Center();
		}
	}

	sealed class DescriptionBodyView : ScrollView
	{
		public DescriptionBodyView()
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
			};
		}
	}

	sealed class IsAuthenticatingIndicator : ActivityIndicator
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