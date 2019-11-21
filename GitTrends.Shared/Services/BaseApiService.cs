using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Xamarin.Essentials;

namespace GitTrends.Shared
{
    public abstract class BaseApiService
    {
        protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, int numRetries = 3)
        {
            return Policy.Handle<Exception>().WaitAndRetryAsync(numRetries, retryAttempt).ExecuteAsync(action);

            static TimeSpan retryAttempt(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
        }

        protected static HttpClient CreateHttpClient(string url)
        {
            HttpClient client;

            if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.Android)
            {
                client = new HttpClient();
            }
            else
            {
                client = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip });
            }

            client.BaseAddress = new Uri(url);

            return client;
        }
    }
}
