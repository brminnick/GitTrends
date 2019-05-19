using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(GitTrends.iOS.AuthenticationServices_iOS))]
namespace GitTrends.iOS
{
    public class AuthenticationServices_iOS : IAuthenticationService
    {
        public async Task LaunchWebAuthentication()
        {
            var currentViewController = await HelperMethods.GetVisibleViewController().ConfigureAwait(false);

            await XamarinFormsServices.BeginInvokeOnMainThreadAsync(() => currentViewController.PresentViewController(AuthenticationService.Authenticator.GetUI(), true, AfterPresentViewController)).ConfigureAwait(false);
        }

        void AfterPresentViewController()
        {

        }
    }
}
