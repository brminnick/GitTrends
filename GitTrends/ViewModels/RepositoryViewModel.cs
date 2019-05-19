using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;

namespace GitTrends
{
    public class RepositoryViewModel : BaseViewModel
    {
        public RepositoryViewModel()
        {
            LoginButtonCommand = new AsyncCommand(GitHubAuthenticationService.LaunchWebAuthenticationPage, continueOnCapturedContext: false);
        }

        public ICommand LoginButtonCommand { get; }
    }
}
