using System.ComponentModel;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using GitTrends.Common;
using GitTrends.Mobile.Common.Constants;
using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

public class ChartOnboardingView(IDeviceInfo deviceInfo, IAnalyticsService analyticsService)
	: BaseOnboardingDataTemplate(
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
		const double _chartVideoHeight = 1080;
		const double _chartVideoWidth = 860;

		public ImageView()
		{
			StrokeShape = new RoundRectangle
			{
				CornerRadius = 4,
			};
			Stroke = Color.FromArgb("E0E0E0");
			BackgroundColor = Colors.White;
			Padding = new Thickness(5);

			Content = new MediaElement
			{
				Source = MediaSource.FromResource(VideoConstants.ChartVideoFileName),
				Background = null,
				ShouldAutoPlay = true,
				ShouldShowPlaybackControls = false,
				ShouldLoopPlayback = true,
				Volume = 0.0,
				Margin = 0,
				HeightRequest = 250, // Assign any value to MediaElement.HeightRequest; workaround to ensure MediaElement is inflated on iOS
				WidthRequest = 250 // Assign any value to MediaElement.WidthRequest; workaround to ensure MediaElement is inflated on iOS
			}.Center();

			PropertyChanged += HandlePropertyChanged;
		}

		static void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			ArgumentNullException.ThrowIfNull(sender);

			var imageView = (ImageView)sender;

			// Ensure both Width + Height have been initialized 
			if ((e.PropertyName == HeightProperty.PropertyName
				|| e.PropertyName == WidthProperty.PropertyName)
				&& imageView.Height is not -1
				&& imageView.Width is not -1)
			{
				var mediaElement = (MediaElement)(imageView.Content ?? throw new InvalidOperationException($"{nameof(ImageView)}.{nameof(Content)} must be set to a MediaElement in the Constructor"));

				mediaElement.HeightRequest = imageView.Height - imageView.Padding.VerticalThickness;
				mediaElement.WidthRequest = mediaElement.HeightRequest / _chartVideoHeight * _chartVideoWidth;
			}
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