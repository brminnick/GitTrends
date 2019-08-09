using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public abstract class BaseMobileApiService : BaseApiService
    {
        static int _networkIndicatorCount = 0;

        protected static string GetGitHubBearerTokenHeader(GitHubToken token) => $"{token.TokenType} {token.AccessToken}";

        protected static void UpdateActivityIndicatorStatus(bool isActivityIndicatorDisplayed)
        {
            if (isActivityIndicatorDisplayed)
            {
                Device.BeginInvokeOnMainThread(() => Application.Current.MainPage.IsBusy = true);
                _networkIndicatorCount++;
            }
            else if (--_networkIndicatorCount <= 0)
            {
                Device.BeginInvokeOnMainThread(() => Application.Current.MainPage.IsBusy = false);
                _networkIndicatorCount = 0;
            }
        }

        protected static async Task<T> ExecuteMobilePollyFunction<T>(Func<Task<T>> action, int numRetries = 3)
        {
            UpdateActivityIndicatorStatus(true);

            try
            {
                return await ExecutePollyFunction(action, numRetries).ConfigureAwait(false);
            }
            finally
            {
                UpdateActivityIndicatorStatus(false);
            }
        }
    }
}
