using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public abstract partial class GitHubAuthenticationViewModel : BaseViewModel
	{
		readonly DeepLinkingService _deepLinkingService;

		[ObservableProperty]
		[AlsoNotifyChangeFor(nameof(IsNotAuthenticating))]
		[AlsoNotifyChangeFor(nameof(IsDemoButtonVisible))]
		[AlsoNotifyCanExecuteFor(ConnectToGitHubButtonCommand)]
		bool _isAuthenticating = false;

		protected GitHubAuthenticationViewModel(IMainThread mainThread,
													IAnalyticsService analyticsService,
													GitHubUserService gitHubUserService,
													DeepLinkingService deepLinkingService,
													GitHubAuthenticationService gitHubAuthenticationService) : base(analyticsService, mainThread)
		{
			GitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;
			GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

			_deepLinkingService = deepLinkingService;

			GitHubUserService = gitHubUserService;
			GitHubAuthenticationService = gitHubAuthenticationService;
		}

		public bool IsNotAuthenticating => !IsAuthenticating;

		public virtual bool IsDemoButtonVisible => !IsAuthenticating && GitHubUserService.Alias != DemoUserConstants.Alias;

		protected GitHubUserService GitHubUserService { get; }
		protected GitHubAuthenticationService GitHubAuthenticationService { get; }

		[ICommand(CanExecute = nameof(IsNotAuthenticating))]
		protected virtual Task DemoButton(string? buttonText)
		{
			IsAuthenticating = true;
			return Task.CompletedTask;
		}

		[ICommand(CanExecute = nameof(IsNotAuthenticating))]
		protected async virtual Task ConnectToGitHubButton(GitHubAuthenticationService gitHubAuthenticationService, GitHubUserService gitHubUserService, CancellationToken cancellationToken, Xamarin.Essentials.BrowserLaunchOptions? browserLaunchOptions = null)
		{
			IsAuthenticating = true;

			// Yield from the Main Thread to allow IsAuthenticating indicator to appear
			await Task.Yield();

			try
			{
				var loginUrl = gitHubAuthenticationService.GetGitHubLoginUrl();

				if (!string.IsNullOrWhiteSpace(loginUrl))
				{
					await _deepLinkingService.OpenBrowser(loginUrl, browserLaunchOptions).ConfigureAwait(false);
				}
				else
				{
					await _deepLinkingService.DisplayAlert("Error", "Couldn't connect to GitHub Login. Check your internet connection and try again", "OK").ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				AnalyticsService.Report(e);
			}
			finally
			{
				IsAuthenticating = false;
			}
		}

		void HandleAuthorizeSessionStarted(object sender, EventArgs e) => IsAuthenticating = true;
		void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e) => IsAuthenticating = false;
	}
}