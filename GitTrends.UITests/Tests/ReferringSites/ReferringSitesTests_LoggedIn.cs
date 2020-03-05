using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    class ReferringSitesTests_LoggedIn : ReferringSitesTests
    {
        protected ReferringSitesTests_LoggedIn(Platform platform) : base(platform, UserType.LoggedIn)
        {
        }
    }
}
