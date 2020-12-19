using GitTrends.Mobile.Common.Constants;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    class AboutPage : BasePage
    {
        public AboutPage(IApp app) : base(app, () => AboutPageConstants.About)
        {
        }
    }
}
