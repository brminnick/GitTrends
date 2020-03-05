using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    class RepositoriesTests_LoggedIn : RepositoriesTests
    {
        public RepositoriesTests_LoggedIn(Platform platform) : base(platform, UserType.LoggedIn)
        {
        }
    }
}
