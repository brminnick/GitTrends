using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

public class ChartOnboardingPage(IDeviceInfo deviceInfo, IAnalyticsService analyticsService)
	: BaseOnboardingContentPage(OnboardingConstants.SkipText, deviceInfo, Color.FromArgb(BaseTheme.CoralColorHex), 1, analyticsService)
{

	enum Row { Title, Zoom, LongPress }
	enum Column { Image, Description }

	protected override View CreateImageView() => new Border
	{
		StrokeShape = new RoundRectangle
		{
			CornerRadius = 4,
		},
		Stroke = Color.FromArgb("E0E0E0"),
		BackgroundColor = Colors.White,
		Padding = new Thickness(5),
		Content = new VideoPlayerWithLoadingIndicatorView(VideoConstants.ChartVideoFileName)
	};

	protected override TitleLabel CreateDescriptionTitleLabel() => new(OnboardingConstants.ChartPage_Title);

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