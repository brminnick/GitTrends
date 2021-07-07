using System;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    public class ChartOnboardingPage : BaseOnboardingContentPage
    {
        readonly MediaElementService _mediaElementService;

        public ChartOnboardingPage(IMainThread mainThread,
                                    IAnalyticsService analyticsService,
                                    MediaElementService mediaElementService)
            : base(OnboardingConstants.SkipText, Color.FromHex(BaseTheme.CoralColorHex), mainThread, 1, analyticsService)
        {
            _mediaElementService = mediaElementService;
        }

        enum Row { Title, Zoom, LongPress }
        enum Column { Image, Description }

        protected override View CreateImageView()
        {
            var videoPlayerView = new VideoPlayerView
            {
                Uri = new Uri(_mediaElementService.OnboardingChartManifest?.HlsUrl)
            };

            MediaElementService.OnboardingChartManifestChanged += HandleOnboardingChartManifestChanged;

            return new PancakeView
            {
                CornerRadius = 4,
                Border = new Border { Color = Color.FromHex("E0E0E0") },
                BackgroundColor = Color.White,
                Padding = new Thickness(5),
                Content = videoPlayerView
            };

            void HandleOnboardingChartManifestChanged(object sender, StreamingManifest? e)
            {
                if (e is not null)
                {
                    MediaElementService.OnboardingChartManifestChanged -= HandleOnboardingChartManifestChanged;
                    videoPlayerView.Uri = new Uri(e.HlsUrl);
                }
            }

        }

        protected override TitleLabel CreateDescriptionTitleLabel() => new TitleLabel(OnboardingConstants.ChartPage_Title);

        protected override View CreateDescriptionBodyView() => new ScrollView
        {
            Content = new Grid
            {
                RowSpacing = 14,

                RowDefinitions = Rows.Define(
                    (Row.Title, Auto),
                    (Row.Zoom, 48),
                    (Row.LongPress, 48)),

                ColumnDefinitions = Columns.Define(
                    (Column.Image, 56),
                    (Column.Description, Star)),

                Children =
                {
                    new BodyLabel(OnboardingConstants.ChartPage_Body_ShowAllTraffic).Row(Row.Title).ColumnSpan(All<Column>()),

                    new BodySvg("zoom_gesture.svg").Row(Row.Zoom).Column(Column.Image),
                    new BodyLabel(OnboardingConstants.ChartPage_Body_ZoomInOut).Row(Row.Zoom).Column(Column.Description),

                    new BodySvg("longpress_gesture.svg").Row(Row.LongPress).Column(Column.Image),
                    new BodyLabel(OnboardingConstants.ChartPage_Body_LongPress).Row(Row.LongPress).Column(Column.Description),
                }
            }
        };
    }
}
