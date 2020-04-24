using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Shared;
using Xamarin.Essentials;

namespace GitTrends
{
    public abstract class GitHubAuthenticationViewModel : BaseViewModel
    {
        bool _isAuthenticating = false;

        protected GitHubAuthenticationViewModel(GitHubAuthenticationService gitHubAuthenticationService,
                                                    DeepLinkingService deepLinkingService,
                                                    AnalyticsService analyticsService) : base(analyticsService)
        {
            gitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;
            gitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

            ConnectToGitHubButtonCommand = new AsyncCommand<CancellationToken>(cancellationToken => ExecuteConnectToGitHubButtonCommand(gitHubAuthenticationService, deepLinkingService, cancellationToken), _ => IsNotAuthenticating);
            DemoButtonCommand = new AsyncCommand<string>(text => ExecuteDemoButtonCommand(text), _ => IsNotAuthenticating);
            GitHubAuthenticationService = gitHubAuthenticationService;
        }

        public IAsyncCommand<CancellationToken> ConnectToGitHubButtonCommand { get; }
        public IAsyncCommand<string> DemoButtonCommand { get; }

        public bool IsNotAuthenticating => !IsAuthenticating;

        public virtual bool IsDemoButtonVisible => !IsAuthenticating && GitHubAuthenticationService.Alias != DemoDataConstants.Alias;

        protected GitHubAuthenticationService GitHubAuthenticationService { get; }

        public bool IsAuthenticating
        {
            get => _isAuthenticating;
            set => SetProperty(ref _isAuthenticating, value, () =>
            {
                NotifyIsAuthenticatingPropertyChanged();
                MainThread.InvokeOnMainThreadAsync(ConnectToGitHubButtonCommand.RaiseCanExecuteChanged).SafeFireAndForget(ex => Debug.WriteLine(ex));
            });
        }

        protected virtual void NotifyIsAuthenticatingPropertyChanged()
        {
            OnPropertyChanged(nameof(IsNotAuthenticating));
            OnPropertyChanged(nameof(IsDemoButtonVisible));
        }

        protected virtual Task ExecuteDemoButtonCommand(string buttonText)
        {
            IsAuthenticating = true;
            return Task.CompletedTask;
        }

        protected async virtual Task ExecuteConnectToGitHubButtonCommand(GitHubAuthenticationService gitHubAuthenticationService, DeepLinkingService deepLinkingService, CancellationToken cancellationToken)
        {
            IsAuthenticating = true;

            try
            {
                var loginUrl = await gitHubAuthenticationService.GetGitHubLoginUrl(cancellationToken).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(loginUrl))
                {
                    await deepLinkingService.OpenBrowser(loginUrl).ConfigureAwait(false);
                }
                else
                {
                    await deepLinkingService.DisplayAlert("Error", "Couldn't connect to GitHub Login. Check your internet connection and try again", "OK").ConfigureAwait(false);
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
        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e)
        {
            FirstRunService.IsFirstRun = false;
            IsAuthenticating = false;
        }
    }
}
