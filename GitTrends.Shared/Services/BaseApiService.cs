using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Refit;

namespace GitTrends.Shared
{
	public abstract class BaseApiService
	{
		public static HttpClient CreateHttpClient(in string url, HttpClient? client = null)
		{
			client ??= new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });

			client.Timeout = TimeSpan.FromSeconds(5);
			client.BaseAddress = new Uri(url);

			return client;
		}

		protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, CancellationToken cancellationToken, int numRetries = 3)
		{
			return Policy.Handle<Exception>(shouldHandleException).WaitAndRetryAsync(numRetries, retryAttempt).ExecuteAsync(token => action(), cancellationToken);

			static TimeSpan retryAttempt(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));

			static bool shouldHandleException(Exception exception)
			{
				if (exception is ApiException apiException)
					return !isForbiddenOrUnauthorized(apiException);

				return true;

				static bool isForbiddenOrUnauthorized(ApiException apiException) => apiException.StatusCode is System.Net.HttpStatusCode.Forbidden || apiException.StatusCode is System.Net.HttpStatusCode.Unauthorized;
			}
		}
	}
}