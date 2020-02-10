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

        static async ValueTask UpdateActivityIndicatorStatus(bool isActivityIndicatorDisplayed)
        {
            if (isActivityIndicatorDisplayed)
            {
                _networkIndicatorCount++;

                if (Xamarin.Essentials.MainThread.IsMainThread)
                    setIsBusy(true);
                else
                    await Device.InvokeOnMainThreadAsync(() => setIsBusy(true));
            }
            else if (--_networkIndicatorCount <= 0)
            {
                _networkIndicatorCount = 0;

                if (Xamarin.Essentials.MainThread.IsMainThread)
                    setIsBusy(false);
                else
                    await Device.InvokeOnMainThreadAsync(() => setIsBusy(false));
            }

            static void setIsBusy(bool isBusy)
            {
                if (Application.Current?.MainPage != null)
                    Application.Current.MainPage.IsBusy = isBusy;
            }
        }
    }
}
