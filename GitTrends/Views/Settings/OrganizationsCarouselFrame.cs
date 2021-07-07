using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Sharpnado.MaterialFrame;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    class OrganizationsCarouselFrame : MaterialFrame
    {
        const int _cornerRadius = 12;

        readonly IndicatorView _indicatorView;
        readonly IAnalyticsService _analyticsService;

        public OrganizationsCarouselFrame(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;

            Opacity = 0; // Keep this view hidden until user toggles the IncludeOrganizations Switch
            Padding = 0;

            CornerRadius = _cornerRadius;

            Margin = new Thickness(16, 32);

            LightThemeBackgroundColor = GetBackgroundColor(0);

            Elevation = 8;

            Content = new EnableOrganizationsGrid
            {
                RowSpacing = 0, // Must be zero to match EnableOrganizationsCarouselTemplateSelector's Grid

                IsClippedToBounds = true,

                Children =
                {
                    new OpacityOverlay()
                        .Row(EnableOrganizationsGrid.Row.Image),

                    new OrganizationsCarouselView()
                        .Row(EnableOrganizationsGrid.Row.Image).RowSpan(All<EnableOrganizationsGrid.Row>())
                        .Invoke(view => view.PositionChanged += HandlePositionChanged)
                        .FillExpand(),

                    new EnableOrganizationsCarouselIndicatorView()
                        .Row(EnableOrganizationsGrid.Row.IndicatorView)
                        .Assign(out _indicatorView)
                }
            };
        }

        static Color GetBackgroundColor(int position) => position % 2 is 0
                                                            ? Color.FromHex(BaseTheme.LightTealColorHex) // Even-numbered Pages are Teal
                                                            : Color.FromHex(BaseTheme.CoralColorHex); // Odd-numbered Pages are Coral

        void HandlePositionChanged(object sender, PositionChangedEventArgs e)
        {
            LightThemeBackgroundColor = GetBackgroundColor(e.CurrentPosition);
            _indicatorView.Position = e.CurrentPosition;

            _analyticsService.Track($"{nameof(OrganizationsCarouselView)} Page {e.CurrentPosition} Appeared");
        }

        class OpacityOverlay : PancakeView
        {
            public OpacityOverlay()
            {
                BackgroundColor = Color.White.MultiplyAlpha(0.25);
                CornerRadius = new CornerRadius(10, 10, 0, 0);
            }
        }

        class OrganizationsCarouselView : CarouselView
        {
            public OrganizationsCarouselView()
            {
                Loop = false;
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never;

                ItemsSource = new[]
                {
                    new IncludeOrganizationsCarouselModel("Title 1", "Text 1", 0, "Business", null),
                    new IncludeOrganizationsCarouselModel("Title 2", "Text 2", 1, "Inspectocat", null),
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
                IndicatorColor = SelectedIndicatorColor.MultiplyAlpha(0.25);

                WidthRequest = 112;
                HeightRequest = 50;

                IndicatorSize = 12;

                AutomationId = SettingsPageAutomationIds.EnableOrangizationsPageIndicator;

                this.Center().Margins(bottom: 12);
            }
        }
    }
}
