using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Autofac;
using GitTrends.Shared;
using Xamarin.Forms;

namespace GitTrends
{
    public abstract class BaseMobileApiService : BaseApiService
    {
        readonly static AnalyticsService _analyticsService;

        static int _networkIndicatorCount;

        static BaseMobileApiService()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            _analyticsService = scope.Resolve<AnalyticsService>();
        }

        protected static string GetGitHubBearerTokenHeader(GitHubToken token) => $"{token.TokenType} {token.AccessToken}";

        protected static async Task<T> AttemptAndRetry_Mobile<T>(Func<Task<T>> action, int numRetries = 3, IDictionary<string, string>? properties = null, [CallerMemberName] string callerName = "")
        {
            await UpdateActivityIndicatorStatus(true).ConfigureAwait(false);

            try
            {
                using var timedEvent = _analyticsService.TrackTime(callerName, properties);
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
