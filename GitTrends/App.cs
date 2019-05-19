using Xamarin.Forms;

namespace GitTrends
{
    public class App : Application
    {
        public App() => MainPage = new NavigationPage(new RepositoryPage());
    }
}
