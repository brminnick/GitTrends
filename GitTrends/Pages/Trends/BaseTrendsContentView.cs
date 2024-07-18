using GitTrends.Mobile.Common;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

abstract class BaseTrendsContentView : Grid
{
	protected BaseTrendsContentView(in Color indicatorColor,
		in IDeviceInfo deviceInfo,
		in int carouselPositionIndex,
		in TrendsPageType trendsPageType,
		in IAnalyticsService analyticsService)
	{
		AnalyticsService = analyticsService;

		ColumnSpacing = 8;
		RowSpacing = 12;

		RowDefinitions = Rows.Define(
			(Row.Header, ViewsClonesStatisticsGrid.StatisticsGridHeight),
			(Row.Indicator, 12),
			(Row.Chart, Star));

		Children.Add(CreateHeaderView().Row(Row.Header));


		Children.Add(new TrendsIndicatorView(carouselPositionIndex, indicatorColor).Fill().Row(Row.Indicator));

		Children.Add(CreateChartView().Assign(out BaseChartView chartView).Row(Row.Chart));

		Children.Add(CreateEmptyDataView()
			.Margin(chartView.Margin)
			.Padding(new Thickness(chartView.Padding.Left + 4, chartView.Padding.Top + 4, chartView.Padding.Right + 4, chartView.Padding.Bottom + 4))
			.Row(Row.Chart));

		Children.Add(new TrendsChartActivityIndicator(trendsPageType, deviceInfo).Row(Row.Chart));
		
		this.Padding(0, 16);
	}

	protected enum TrendsPageType { ViewsClonesTrendsPage, StarsTrendsPage }
	enum Row { Header, Indicator, Chart }
	
	protected IAnalyticsService AnalyticsService { get; }

	protected abstract Layout CreateHeaderView();
	protected abstract BaseChartView CreateChartView();
	protected abstract EmptyDataView CreateEmptyDataView();

	sealed class TrendsIndicatorView : IndicatorView
	{
		public TrendsIndicatorView(in int position, in Color indicatorColor)
		{
			Position = position;

			IndicatorSize = 8;

			IsEnabled = false;

			SelectedIndicatorColor = indicatorColor;
			IndicatorColor = Color.FromArgb("#BFBFBF");
			AutomationId = TrendsPageAutomationIds.IndicatorView;


			this.Center();


			SetBinding(CountProperty, new Binding(nameof(TrendsPage.PageCount),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(TrendsPage))));
		}
	}

	sealed class TrendsChartActivityIndicator : ActivityIndicator
	{
		public TrendsChartActivityIndicator(TrendsPageType trendsPageType, IDeviceInfo deviceInfo)
		{
			//The size of UIActivityIndicator is fixed by iOS, so we'll use Xamarin.Forms.VisualElement.Scale to increase its size
			//https://stackoverflow.com/a/2638224/5953643
			if (deviceInfo.Platform == DevicePlatform.iOS)
				Scale = 2;

			AutomationId = TrendsPageAutomationIds.ActivityIndicator;

			var isFetchingDataPath = trendsPageType switch
			{
				TrendsPageType.StarsTrendsPage => nameof(TrendsViewModel.IsFetchingStarsData),
				TrendsPageType.ViewsClonesTrendsPage => nameof(TrendsViewModel.IsFetchingViewsClonesData),
				_ => throw new NotImplementedException()
			};

			this.Center()
				.DynamicResource(ColorProperty, nameof(BaseTheme.ActivityIndicatorColor))
				.Bind(IsVisibleProperty, isFetchingDataPath)
				.Bind(IsRunningProperty, isFetchingDataPath);
		}
	}
}