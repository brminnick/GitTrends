using System.Collections.Generic;
using GitTrends.Mobile.Shared;
using Xamarin.UITest;

namespace GitTrends.UITests
{
    abstract class BaseCollectionPage<TCollection> : BasePage
    {
        protected BaseCollectionPage(IApp app, string pageTitle = "") : base(app, pageTitle)
        {

        }

        public List<TCollection> VisibleCollection => App.InvokeBackdoorMethod<List<TCollection>>(BackdoorMethodConstants.GetVisibleCollection);

        public void TriggerPullToRefresh() => App.InvokeBackdoorMethod(BackdoorMethodConstants.TriggerPullToRefresh);
    }
}
