using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class ChartOnboardingPage : BaseOnboardingContentPage
    {
        public ChartOnboardingPage(IAnalyticsService analyticsService, IMainThread mainThread) : base(analyticsService, mainThread, Color.FromHex(BaseTheme.CoralColorHex), OnboardingConstants.SkipText, 1)
        {
        }

        enum Row { Title, Zoom, LongPress }
        enum Column { Image, Description }

        protected override View CreateImageView() => new PancakeView
        {
            CornerRadius = 4,
            BorderColor = Color.FromHex("E0E0E0"),
            BackgroundColor = Color.White,
            Padding = new Thickness(5),

            //On iOS, use custom renderer for MediaElement until MediaElement.Dispose bug is fixed: https://github.com/xamarin/Xamarin.Forms/issues/9525#issuecomment-619156536
            //On Android, use Custom Renderer for ExoPlayer because Xamarin.Forms.MediaElement uses Android.VideoView
            Content = new VideoPlayerView(),

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
