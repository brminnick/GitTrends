using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

public class ChartOnboardingView(IDeviceInfo deviceInfo, IAnalyticsService analyticsService)
	: BaseOnboardingContentView(
		OnboardingConstants.SkipText,
		deviceInfo,
		Color.FromArgb(BaseTheme.CoralColorHex),
		1,
		() => new ImageView(),
		() => new TitleLabel(OnboardingConstants.ChartPage_Title),
		() => new DescriptionBodyView(deviceInfo),
		analyticsService)
{

	enum Row { Title, Zoom, LongPress }
	enum Column { Image, Description }

	sealed class ImageView : Border
	{
		public ImageView()
		{
			const int chartVideoHeight = 1080;
			const int chartVideoWidth = 860;

			StrokeShape = new RoundRectangle
			{
				CornerRadius = 4,
			};
			Stroke = Color.FromArgb("E0E0E0");
			BackgroundColor = Colors.White;
			Padding = new Thickness(5);
			
#if AppStore
#error 
#endif
#if !(IOS || !MACCATALYST)

			Content = new MediaElement
				{
					Source = MediaSource.FromResource(VideoConstants.ChartVideoFileName),
					Background = null,
					ShouldAutoPlay = true,
					ShouldShowPlaybackControls = false,
					ShouldLoopPlayback = true,
					Volume = 0.0,
					Margin = 0,
				}.Center()
				.Bind(WidthRequestProperty,
					source: this,
					getter: imageView => imageView.Height,
					convert: convertWidthToMatchRecordedVideoDimensions);

			// This ensures that black bars don't appear on the sides of the video due to being improperly scaled
			double convertWidthToMatchRecordedVideoDimensions(double imageViewHeight) => (imageViewHeight - Padding.VerticalThickness) / chartVideoHeight * chartVideoWidth;
#endif
		}
	}

	sealed class DescriptionBodyView : ScrollView
	{
		public DescriptionBodyView(in IDeviceInfo deviceInfo)
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

					new BodySvg(deviceInfo, "zoom_gesture.svg").Row(Row.Zoom).Column(Column.Image),
					new BodyLabel(OnboardingConstants.ChartPage_Body_ZoomInOut).Row(Row.Zoom).Column(Column.Description),

					new BodySvg(deviceInfo, "longpress_gesture.svg").Row(Row.LongPress).Column(Column.Image),
					new BodyLabel(OnboardingConstants.ChartPage_Body_LongPress).Row(Row.LongPress).Column(Column.Description),
				}
			};
		}
	}
}