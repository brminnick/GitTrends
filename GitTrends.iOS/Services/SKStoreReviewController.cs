using System.Threading.Tasks;
using GitTrends.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(SKStoreReviewController))]
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
