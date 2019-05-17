using Xamarin.Forms;

namespace GitTrends
{
    public class RepositoryPage : BaseContentPage<RepositoryViewModel>
    {
        public RepositoryPage()
        {
            var gitHubLoginButton = new Button { Text = "Login with GitHub" };
            gitHubLoginButton.SetBinding(Button.CommandProperty, nameof(ViewModel.LoginButtonCommand));

            Content = gitHubLoginButton;
        }
    }
}
