using System;
using System.Diagnostics;
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
            ConnectToGitHubButtonTapped = new AsyncCommand(() => ExecuteConnectToGitHubButtonTapped(gitHubAuthenticationService, deepLinkingService), _ => !IsAuthenticating);
        }

        public IAsyncCommand ConnectToGitHubButtonTapped { get; }

        public virtual bool IsDemoButtonVisible => !IsAuthenticating && GitHubAuthenticationService.Alias != DemoDataConstants.Alias;

        public bool IsAuthenticating
        {
            get => _isAuthenticating;
            set => SetProperty(ref _isAuthenticating, value, () =>
            {
                OnPropertyChanged(nameof(IsDemoButtonVisible));
                MainThread.InvokeOnMainThreadAsync(ConnectToGitHubButtonTapped.RaiseCanExecuteChanged).SafeFireAndForget(ex => Debug.WriteLine(ex));
            });
        }

        protected async virtual Task ExecuteConnectToGitHubButtonTapped(GitHubAuthenticationService gitHubAuthenticationService, DeepLinkingService deepLinkingService)
        {
            IsAuthenticating = true;

            try
            {
                var loginUrl = await gitHubAuthenticationService.GetGitHubLoginUrl().ConfigureAwait(false);

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
                IsAuthenticating = false;
            }
        }

        void HandleAuthorizeSessionStarted(object sender, EventArgs e) => IsAuthenticating = true;
        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e)
        {
            IsAuthenticating = false;
            FirstRunService.IsFirstRun = false;
        }
    }
}
