using GitTrends.Mobile.Common;
using Sharpnado.MaterialFrame;
using SkiaSharp;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;

namespace GitTrends
{
    class OrganizationsCarouselFrame : MaterialFrame
    {
        readonly IndicatorView _indicatorView;

        public OrganizationsCarouselFrame()
        {
            Opacity = 0;
            Padding = 0;
            CornerRadius = 10;
            Margin = new Thickness(16, 32);
            LightThemeBackgroundColor = GetBackgroundColor(0);

            Elevation = 8;

            Content = new Label { Text = "Brandon" }.Center();

            Content = new StackLayout
            {
                IsClippedToBounds = true,

                Children =
                {
                    new OrganizationsCarouselView()
                        .Invoke(view => view.PositionChanged += HandlePositionChanged)
                        .FillExpand(),

                    new EnableOrganizationsCarouselIndicatorView()
                        .Assign(out _indicatorView)
                        .Bottom()
                }
            }.FillExpand();
        }

        enum Row { CarouselView, Indicator }

        static Color GetBackgroundColor(int position) => position % 2 is 0
                                                            ? Color.FromHex(BaseTheme.LightTealColorHex) // Even-numbered Pages are Teal
                                                            : Color.FromHex(BaseTheme.CoralColorHex); // Odd-numbered Pages are Coral

        void HandlePositionChanged(object sender, PositionChangedEventArgs e)
        {
            LightThemeBackgroundColor = GetBackgroundColor(e.CurrentPosition);
            _indicatorView.Position = e.CurrentPosition;
        }

        class OrganizationsCarouselView : CarouselView
        {
            public OrganizationsCarouselView()
            {
                Loop = false;
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never;

                ItemsSource = new[]
                {
                    new IncludeOrganizationsCarouselModel("Title 1", "Text 1", 0, null, null),
                    new IncludeOrganizationsCarouselModel("Title 2", "Text 2", 1, null, null),
                    new IncludeOrganizationsCarouselModel("Title 3", "Text 3", 2, null, null),
                };

                ItemTemplate = new EnableOrganizationsCarouselTemplateSelector();
            }
        }

        class EnableOrganizationsCarouselIndicatorView : IndicatorView
        {
            public EnableOrganizationsCarouselIndicatorView()
            {
                Count = 3;
                IsEnabled = false;
                SelectedIndicatorColor = Color.White;
                IndicatorColor = Color.White.MultiplyAlpha(0.25);

                WidthRequest = 112;
                HeightRequest = 50;

                IndicatorSize = 12;

                AutomationId = SettingsPageAutomationIds.EnableOrangizationsPageIndicator;

                this.Center().Margins(bottom: 24);
            }
        }
    }
}
