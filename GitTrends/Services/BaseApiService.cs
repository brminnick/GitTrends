using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Xamarin.Forms;

namespace GitTrends
{
    abstract class BaseApiService
    {
        #region Fields
        static int _networkIndicatorCount = 0;
        #endregion

        #region Methods
        protected async static Task<T> ExecutePollyFunction<T>(Func<Task<T>> action, int numRetries = 3)
        {
            UpdateActivityIndicatorStatus(true);

            try
            {
                return await Policy.Handle<Exception>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).ExecuteAsync(action).ConfigureAwait(false);
            }
            finally
            {
                UpdateActivityIndicatorStatus(false);
            }

            TimeSpan pollyRetryAttempt(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
        }

        protected static HttpClient CreateHttpClient(string url)
        {
            HttpClient client;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                case Device.Android:
                    client = new HttpClient();
                    break;
                default:
                    client = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip });
                    break;
            }
            client.BaseAddress = new Uri(url);

            return client;
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
