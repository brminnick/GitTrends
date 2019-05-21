using System;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    abstract class BaseMobileApiService : BaseApiService
    {
        #region Fields
        static int _networkIndicatorCount = 0;
        #endregion

        #region Methods
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
        #endregion
    }
}
