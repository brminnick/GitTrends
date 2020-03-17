using System.Threading.Tasks;
using GitTrends;
using Xamarin.Forms;

[assembly: Dependency(typeof(ISKStoreReviewController))]
namespace GitTrends.iOS
{
    public class SKStoreReviewController : ISKStoreReviewController
    {
        public Task RequestReview()
        {
#if AppStore
            return Xamarin.Essentials.MainThread.InvokeOnMainThreadAsync(StoreKit.SKStoreReviewController.RequestReview);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
