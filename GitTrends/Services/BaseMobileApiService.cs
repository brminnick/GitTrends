using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public abstract class BaseMobileApiService : BaseApiService
    {
        readonly IMainThread _mainThread;

        static int _networkIndicatorCount;

        protected BaseMobileApiService(IAnalyticsService analyticsService, IMainThread mainThread) =>
            (AnalyticsService, _mainThread) = (analyticsService, mainThread);

        protected IAnalyticsService AnalyticsService { get; }

        protected string GetGitHubBearerTokenHeader(GitHubToken token)
        {
            if (token.IsEmpty())
                throw new Exception("Token Cannot Be Empty");

            if (string.IsNullOrWhiteSpace(token.TokenType))
                throw new Exception("Invalid TokenType");

            if (string.IsNullOrWhiteSpace(token.AccessToken))
                throw new Exception("Invalid AccessToken");

            return $"{token.TokenType} {token.AccessToken}";
        }

        protected async Task<T> AttemptAndRetry_Mobile<T>(Func<Task<T>> action, CancellationToken cancellationToken, int numRetries = 3, IDictionary<string, string>? properties = null, [CallerMemberName] string callerName = "")
        {
            await UpdateActivityIndicatorStatus(true).ConfigureAwait(false);

            try
            {
                using var timedEvent = AnalyticsService.TrackTime(callerName, properties);
                return await AttemptAndRetry(action, cancellationToken, numRetries).ConfigureAwait(false);
            }
            finally
            {
                await UpdateActivityIndicatorStatus(false).ConfigureAwait(false);
            }
        }

        async ValueTask UpdateActivityIndicatorStatus(bool isActivityIndicatorDisplayed)
        {
            if (isActivityIndicatorDisplayed)
            {
                _networkIndicatorCount++;

                await setIsBusy(true).ConfigureAwait(false);
            }
            else if (--_networkIndicatorCount <= 0)
            {
                _networkIndicatorCount = 0;

                await setIsBusy(false).ConfigureAwait(false);
            }

            async ValueTask setIsBusy(bool isBusy)
            {
                if (Xamarin.Forms.Application.Current?.MainPage is Xamarin.Forms.Page mainPage)
                    await _mainThread.InvokeOnMainThreadAsync(() => mainPage.IsBusy = isBusy).ConfigureAwait(false);
            }
        }
    }
}
