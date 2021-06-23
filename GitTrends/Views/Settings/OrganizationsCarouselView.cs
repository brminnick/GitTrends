using Xamarin.Forms;

namespace GitTrends
{
    class OrganizationsCarouselView : CarouselView
    {
        public OrganizationsCarouselView()
        {
            Opacity = 0;

            ItemTemplate = new EnableOrganizationsCarouselTemplateSelector();

            ItemsSource = new[]
            {
                new IncludeOrganizationsCarouselModel("Title 1", "Text 1", Color.FromHex(BaseTheme.LightTealColorHex), 0, null, null),
                new IncludeOrganizationsCarouselModel("Title 2", "Text 2", Color.FromHex(BaseTheme.CoralColorHex), 1, null, null),
                new IncludeOrganizationsCarouselModel("Title 3", "Text 3", Color.FromHex(BaseTheme.LightTealColorHex), 2, null, null),
            };
        }
    }
}
