using System;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;

namespace GitTrends
{
    class OrganizationsCarouselView : CarouselView
    {
        readonly static AsyncAwaitBestPractices.WeakEventManager _launchOrganizationsButtonTappedEventManager = new();

        public OrganizationsCarouselView()
        {
            HeightRequest = XamarinFormsService.ScreenHeight * 2 / 3;

            ItemTemplate = new EnableOrganizationsCarouselTemplate();

            ItemsSource = new[]
            {
                new ScrollToBottomOrganizationsView(),
                new ScrollToBottomOrganizationsView(),
                new ScrollToBottomOrganizationsView()
                    .Invoke(view => view.LaunchOrganizationsButtonTapped += HandleLaunchOrganizationsButtonTapped),
            };
        }


        public static event EventHandler LaunchOrganizationsButtonTapped
        {
            add => _launchOrganizationsButtonTappedEventManager.AddEventHandler(value);
            remove => _launchOrganizationsButtonTappedEventManager.RemoveEventHandler(value);
        }        

        void HandleLaunchOrganizationsButtonTapped(object sender, EventArgs e) =>
            _launchOrganizationsButtonTappedEventManager.RaiseEvent(this, EventArgs.Empty, nameof(LaunchOrganizationsButtonTapped));
    }
}
