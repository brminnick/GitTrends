using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class AboutPage : BaseContentPage
    {
        public AboutPage(IAnalyticsService analyticsService, IMainThread mainThread) : base(analyticsService, mainThread)
        {
            Title = AboutPageConstants.About;
            Content = new Label { Text = DemoDataConstants.GetRandomText(255) }.Center().TextCenter();
        }
    }
}
