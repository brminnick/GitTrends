using System;
using Xamarin.Forms;

namespace GitTrends
{
    class OrganizationsCarouselView : CarouselView
    {
        readonly static AsyncAwaitBestPractices.WeakEventManager _launchOrganizationsButtonTappedEventManager = new();

        public OrganizationsCarouselView()
        {
            ItemTemplate = new EnableOrganizationsCarouselTemplateSelector();

            ItemsSource = new[]
            {
                new IncludeOrganizationsCarouselModel("Title 1", "Text 1", Color.FromHex(BaseTheme.LightTealColorHex), 0, null, null),
                new IncludeOrganizationsCarouselModel("Title 2", "Text 2", Color.FromHex(BaseTheme.CoralColorHex), 1, null, null),
                new IncludeOrganizationsCarouselModel("Title 3", "Text 3", Color.FromHex(BaseTheme.LightTealColorHex), 2, null, null),
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
