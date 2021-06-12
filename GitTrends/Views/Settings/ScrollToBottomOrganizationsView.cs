using System;
using Xamarin.Forms;

namespace GitTrends
{
    class ScrollToBottomOrganizationsView : View
    {
        readonly AsyncAwaitBestPractices.WeakEventManager _launchOrganizationsButtonTappedEventManager = new();

        public ScrollToBottomOrganizationsView()
        {
        }

        public event EventHandler LaunchOrganizationsButtonTapped
        {
            add => _launchOrganizationsButtonTappedEventManager.AddEventHandler(value);
            remove => _launchOrganizationsButtonTappedEventManager.RemoveEventHandler(value);
        }

        void OnLaunchOrganizationsButtonTapped() => _launchOrganizationsButtonTappedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(LaunchOrganizationsButtonTapped));
    }
}
