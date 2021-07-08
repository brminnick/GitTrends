using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Sharpnado.MaterialFrame;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    class OrganizationsCarouselFrame : MaterialFrame
    {
        const int _cornerRadius = 12;

        readonly IMainThread _mainThread;
        readonly IndicatorView _indicatorView;
        readonly IAnalyticsService _analyticsService;

        public OrganizationsCarouselFrame(IDeviceInfo deviceInfo,
                                            IMainThread mainThread,
                                            IAnalyticsService analyticsService,
                                            MediaElementService mediaElementService)
        {
            _mainThread = mainThread;
            _analyticsService = analyticsService;

            Padding = 0;

            CornerRadius = _cornerRadius;

            Margin = new Thickness(16, 32);

            LightThemeBackgroundColor = GetBackgroundColor(0);

            Elevation = 8;

            Content = new EnableOrganizationsGrid
            {
                IsClippedToBounds = true,

                Children =
                {
                    new OpacityOverlay()
                        .Row(EnableOrganizationsGrid.Row.Image),

                    new OrganizationsCarouselView(deviceInfo, mediaElementService)
                        .Row(EnableOrganizationsGrid.Row.Image).RowSpan(All<EnableOrganizationsGrid.Row>())
                        .Invoke(view => view.PositionChanged += HandlePositionChanged)
                        .FillExpand(),

                    new EnableOrganizationsCarouselIndicatorView()
                        .Row(EnableOrganizationsGrid.Row.IndicatorView)
                        .Assign(out _indicatorView)
                }
            };

            Dismiss(false).SafeFireAndForget(ex => analyticsService.Report(ex));
        }

        public Task Reveal(bool shouldAnimate) => _mainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (shouldAnimate)
                await this.FadeTo(1);
            else
                Opacity = 1;

            InputTransparent = Content.InputTransparent = false;
        });

        public Task Dismiss(bool shouldAnimate) => _mainThread.InvokeOnMainThreadAsync(async () =>
        {
            InputTransparent = Content.InputTransparent = true;

            if (shouldAnimate)
                await this.FadeTo(0, 1500);
            else
                Opacity = 0;
        });

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
                CornerRadius = new CornerRadius(_cornerRadius, _cornerRadius, 0, 0);
            }
        }

        class OrganizationsCarouselView : CarouselView
        {
            public OrganizationsCarouselView(IDeviceInfo deviceInfo,
                                                MediaElementService mediaElementService)
            {
                Loop = false;
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never;

                ItemsSource = new[]
                {
                    new IncludeOrganizationsCarouselModel("Title 1", "Text 1", 0, "Business", null),
                    new IncludeOrganizationsCarouselModel("Title 2", "Text 2", 1, "Inspectocat", null),
                    new IncludeOrganizationsCarouselModel("Title 3", "Text 3", 2, null, deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS
                                                                                        ? mediaElementService.EnableOrganizationsManifest?.HlsUrl
                                                                                        : mediaElementService.EnableOrganizationsManifest?.ManifestUrl),
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
