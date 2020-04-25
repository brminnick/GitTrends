using System;
using GitTrends.Mobile.Shared;
using MediaManager.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class ChartOnboardingPage : BaseOnboardingContentPage
    {
        public ChartOnboardingPage(AnalyticsService analyticsService) : base(analyticsService, Color.FromHex(BaseTheme.CoralColorHex), OnboardingConstants.SkipText, 1)
        {
        }

        enum Row { Title, Zoom, LongPress }
        enum Column { Image, Description }

        protected override View CreateImageView() => new PancakeView
        {
            CornerRadius = 4,
            BorderColor = Color.FromHex("E0E0E0"),
            BorderThickness = 1,
            BackgroundColor = Color.White,
            Padding = new Thickness(5),

            Content = Device.RuntimePlatform switch
            {
                //Use MediaManager.Forms.VideoView until MediaElement.Dispose bug is fixed: https://github.com/xamarin/Xamarin.Forms/issues/9525#issuecomment-619156536
                Device.iOS => new VideoView
                {
                    Source = MediaElementService.OnboardingChart?.HlsUrl,
                    BackgroundColor = Color.Transparent,
                    Volume = 0,
                    Repeat = MediaManager.Playback.RepeatMode.All,
                    VideoAspect = MediaManager.Video.VideoAspectMode.AspectFit,
                },
                //Work-around because Xamarin.Forms.MediaElement uses Android.VideoView instead of ExoPlayer
                Device.Android => new AndroidExoPlayerView
                {
                    SourceUrl = MediaElementService.OnboardingChart?.ManifestUrl
                },
                _ => throw new NotSupportedException()
            }
        };

        protected override TitleLabel CreateDescriptionTitleLabel() => new TitleLabel(OnboardingConstants.ChartPageTitle);

        protected override View CreateDescriptionBodyView() => new Grid
        {
            RowSpacing = 14,

            RowDefinitions = Rows.Define(
                (Row.Title, AbsoluteGridLength(20)),
                (Row.Zoom, AbsoluteGridLength(48)),
                (Row.LongPress, AbsoluteGridLength(48))),

            ColumnDefinitions = Columns.Define(
                (Column.Image, AbsoluteGridLength(56)),
                (Column.Description, Star)),

            Children =
            {
                new BodyLabel("Charts show all traffic related to your repo:").Row(Row.Title).ColumnSpan(All<Column>()),

                new BodySvg("zoom_gesture.svg").Row(Row.Zoom).Column(Column.Image),
                new BodyLabel("Zoom in/out to see accurately what you need to know").Row(Row.Zoom).Column(Column.Description),

                new BodySvg("longpress_gesture.svg").Row(Row.LongPress).Column(Column.Image),
                new BodyLabel("Long press on the chart to see precise numeric values").Row(Row.LongPress).Column(Column.Description),
            }
        };
    }
}
