using System;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Forms;

namespace GitTrends
{
    public static class AuthenticationService
    {
        readonly static Lazy<OAuth2Authenticator> _authenticatorHolder = new Lazy<OAuth2Authenticator>(() =>
        {
            var authenticator = new OAuth2Authenticator("cd7d3240c298193b55de", "repo", new Uri("https://github.com/login/oauth/authorize"), new Uri("gittrends://"));
            authenticator.Completed += HandleAuthenticatorCompleted;
            authenticator.Error += HandleAuthenticatorError;

            return authenticator;
        });

        public static OAuth2Authenticator Authenticator => _authenticatorHolder.Value;

        public static Task LaunchWebAuthentication() => DependencyService.Get<IAuthenticationService>()?.LaunchWebAuthentication();

        static void HandleAuthenticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            if (sender is OAuth2Authenticator)
            {

            }
        }

        static void HandleAuthenticatorCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            if (sender is OAuth2Authenticator)
            {

            }
        }
    }
}
