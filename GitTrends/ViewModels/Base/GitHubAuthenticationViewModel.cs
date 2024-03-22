using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends;

public abstract partial class GitHubAuthenticationViewModel : BaseViewModel
{
	readonly DeepLinkingService _deepLinkingService;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(HandleConnectToGitHubButtonCommand))]
	[NotifyPropertyChangedFor(nameof(IsNotAuthenticating), nameof(IsDemoButtonVisible))]
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

	protected virtual void NotifyIsAuthenticatingPropertyChanged()
	{
	}

	[RelayCommand(CanExecute = nameof(IsNotAuthenticating))]
	protected virtual Task HandleDemoButtonTapped(string? buttonText)
	{
		IsAuthenticating = true;
		return Task.CompletedTask;
	}

	[RelayCommand(CanExecute = nameof(IsNotAuthenticating))]
	protected virtual async Task HandleConnectToGitHubButton((CancellationToken cancellationToken, Xamarin.Essentials.BrowserLaunchOptions? browserLaunchOptions) parameter)
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

	void HandleAuthorizeSessionStarted(object sender, EventArgs e) => IsAuthenticating = true;
	void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e) => IsAuthenticating = false;

	partial void OnIsAuthenticatingChanged(bool value) => NotifyIsAuthenticatingPropertyChanged();
}