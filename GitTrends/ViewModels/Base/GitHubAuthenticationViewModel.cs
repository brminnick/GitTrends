using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public abstract partial class GitHubAuthenticationViewModel : BaseViewModel
	{
		readonly DeepLinkingService _deepLinkingService;

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

		public bool IsAuthenticating
		{
			get => _isAuthenticating;
			set => SetProperty(_isAuthenticating, value, _ =>
			{
				NotifyIsAuthenticatingPropertyChanged();
				MainThread.InvokeOnMainThreadAsync(HandleConnectToGitHubButtonCommand.NotifyCanExecuteChanged).SafeFireAndForget(ex => Debug.WriteLine(ex));
			});
		}

		protected GitHubUserService GitHubUserService { get; }
		protected GitHubAuthenticationService GitHubAuthenticationService { get; }

		[ICommand(CanExecute = nameof(IsNotAuthenticating))]
		protected virtual Task HandleDemoButtonTapped(string? buttonText)
		{
			IsAuthenticating = true;
			return Task.CompletedTask;
		}

		[ICommand(CanExecute = nameof(IsNotAuthenticating))]
		protected async virtual Task HandleConnectToGitHubButton((CancellationToken cancellationToken, Xamarin.Essentials.BrowserLaunchOptions? browserLaunchOptions) parameter)
		{
			var (cancellationToken, browserLaunchOptions) = parameter;

			IsAuthenticating = true;

			// Yield from the Main Thread to allow IsAuthenticating indicator to appear
			await Task.Yield();

			try
			{
				var loginUrl = GitHubAuthenticationService.GetGitHubLoginUrl();

				cancellationToken.ThrowIfCancellationRequested();

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

		protected virtual void NotifyIsAuthenticatingPropertyChanged()
		{
			OnPropertyChanged(nameof(IsNotAuthenticating));
			OnPropertyChanged(nameof(IsDemoButtonVisible));
		}

		void HandleAuthorizeSessionStarted(object sender, EventArgs e) => IsAuthenticating = true;
		void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e) => IsAuthenticating = false;
	}
}