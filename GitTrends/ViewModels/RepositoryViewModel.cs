using System;
using System.Windows.Input;

using Xamarin.Auth;
using Xamarin.Forms;

namespace GitTrends
{
    public class RepositoryViewModel : BaseViewModel
    {
        public RepositoryViewModel()
        {
            LoginButtonCommand = new Command(ExecuteLoginButtonCommand);
        }

        public ICommand LoginButtonCommand { get; }

        void ExecuteLoginButtonCommand()
        {
            var accounts = AccountStore.Create();

            var authenticator = new OAuth2Authenticator("cd7d3240c298193b55de", "repo", new Uri("https://github.com/login/oauth/authorize"), new Uri("https://github.com/login/oauth/access_token"));

            authenticator.Completed += OnAuthCompleted;
            authenticator.Error += OnAuthError;

            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            presenter.Login(authenticator);
        }

        void OnAuthCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            if (sender is OAuth2Authenticator authenticator)
            {
                authenticator.Completed -= OnAuthCompleted;
                authenticator.Error -= OnAuthError;
            }
        }

        void OnAuthError(object sender, AuthenticatorErrorEventArgs e)
        {
            if (sender is OAuth2Authenticator authenticator)
            {
                authenticator.Completed -= OnAuthCompleted;
                authenticator.Error -= OnAuthError;
            }
        }
    }
}
