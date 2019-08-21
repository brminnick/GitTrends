using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public abstract class BaseMobileApiService : BaseApiService
    {
        static int _networkIndicatorCount;

        protected static string GetGitHubBearerTokenHeader(GitHubToken token) => $"{token.TokenType} {token.AccessToken}";

        protected static async Task UpdateActivityIndicatorStatus(bool isActivityIndicatorDisplayed)
        {
            if (isActivityIndicatorDisplayed)
            {
                _networkIndicatorCount++;
                await Device.InvokeOnMainThreadAsync(() => Application.Current.MainPage.IsBusy = true).ConfigureAwait(false);
            }
            else if (--_networkIndicatorCount <= 0)
            {
                _networkIndicatorCount = 0;
                await Device.InvokeOnMainThreadAsync(() => Application.Current.MainPage.IsBusy = false).ConfigureAwait(false);
            }
        }

        protected static async Task<T> AttemptAndRetry_Mobile<T>(Func<Task<T>> action, int numRetries = 3)
        {
            await UpdateActivityIndicatorStatus(true).ConfigureAwait(false);

            try
            {
                return await AttemptAndRetry(action, numRetries).ConfigureAwait(false);
            }
            finally
            {
                await UpdateActivityIndicatorStatus(false).ConfigureAwait(false);
            }
        }
    }
}
