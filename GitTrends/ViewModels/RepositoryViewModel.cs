using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;

namespace GitTrends
{
    public class RepositoryViewModel : BaseViewModel
    {
        public RepositoryViewModel()
        {
            LoginButtonCommand = new AsyncCommand(AuthenticationService.LaunchWebAuthentication, continueOnCapturedContext: false);
        }

        public ICommand LoginButtonCommand { get; }
    }
}
