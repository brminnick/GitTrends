using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Common;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends;

public abstract partial class GitHubAuthenticationViewModel : BaseViewModel
{
	readonly DeepLinkingService _deepLinkingService;

    protected GitHubAuthenticationViewModel(IDispatcher dispatcher,
												IAnalyticsService analyticsService,
												GitHubUserService gitHubUserService,
												DeepLinkingService deepLinkingService,
												GitHubAuthenticationService gitHubAuthenticationService) : base(analyticsService, dispatcher)
	{
		GitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;
		GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

		_deepLinkingService = deepLinkingService;

		GitHubUserService = gitHubUserService;
		GitHubAuthenticationService = gitHubAuthenticationService;
	}

	public bool IsNotAuthenticating => !IsAuthenticating;

	public virtual bool IsDemoButtonVisible => !IsAuthenticating && GitHubUserService.Alias != DemoUserConstants.Alias;
	
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(HandleConnectToGitHubButtonCommand))]
	[NotifyPropertyChangedFor(nameof(IsNotAuthenticating), nameof(IsDemoButtonVisible))]
	public partial bool IsAuthenticating { get; set; }

	protected GitHubUserService GitHubUserService { get; }
	protected GitHubAuthenticationService GitHubAuthenticationService { get; }

	protected virtual void NotifyIsAuthenticatingPropertyChanged()
	{
	}

	[RelayCommand(CanExecute = nameof(IsNotAuthenticating))]
	protected virtual Task HandleDemoButtonTapped(string? buttonText, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		IsAuthenticating = true;
		return Task.CompletedTask;
	}

	[RelayCommand(CanExecute = nameof(IsNotAuthenticating))]
	protected virtual async Task HandleConnectToGitHubButton((CancellationToken cancellationToken, BrowserLaunchOptions? browserLaunchOptions) parameter)
	{
		var (cancellationToken, browserLaunchOptions) = parameter;

		IsAuthenticating = true;

		// Yield from the Main Thread to allow IsAuthenticating indicator to appear
		await Task.Yield();

		try
		{
			var loginUrl = GitHubAuthenticationService.GetGitHubLoginUrl();

			if (!string.IsNullOrWhiteSpace(loginUrl))
			{
				await _deepLinkingService.OpenBrowser(loginUrl, cancellationToken, browserLaunchOptions).ConfigureAwait(false);
			}
			else
			{
				await _deepLinkingService.DisplayAlert("Error", "Couldn't connect to GitHub Login. Check your internet connection and try again", "OK", cancellationToken).ConfigureAwait(false);
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

	void HandleAuthorizeSessionStarted(object? sender, EventArgs e) => IsAuthenticating = true;
	void HandleAuthorizeSessionCompleted(object? sender, AuthorizeSessionCompletedEventArgs e) => IsAuthenticating = false;

	partial void OnIsAuthenticatingChanged(bool value) => NotifyIsAuthenticatingPropertyChanged();
}