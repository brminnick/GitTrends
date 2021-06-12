using Xamarin.Forms;

namespace GitTrends
{
    class OrganizationsCarouselView : CarouselView
    {
        public OrganizationsCarouselView()
        {
            ItemsSource = new[]
            {
                new ScrollToBottomOrganizationsView(),
            };
        }
    }
}
