using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

abstract class BaseTrendsDataTemplate(
	Color indicatorColor,
	IDeviceInfo deviceInfo,
	int carouselPositionIndex,
	Func<Layout> createHeaderView,
	IAnalyticsService analyticsService,
	Func<BaseChartView> createChartView,
	Func<EmptyDataView> createEmptyDataView,
	BaseTrendsDataTemplate.TrendsPageType trendsPageType)
	: DataTemplate(() => CreateGrid(indicatorColor, deviceInfo, carouselPositionIndex, trendsPageType, createHeaderView, createChartView, createEmptyDataView))
{
	static Grid CreateGrid(
		Color indicatorColor,
		IDeviceInfo deviceInfo,
		int carouselPositionIndex,
		TrendsPageType trendsPageType,
		Func<Layout> createHeaderView,
		Func<BaseChartView> createChartView,
		Func<EmptyDataView> createEmptyDataView) => new Grid
		{
			ColumnSpacing = 8,
			RowSpacing = 12,

			RowDefinitions = Rows.Define(
			(Row.Header, ViewsClonesStatisticsGrid.StatisticsGridHeight),
			(Row.Indicator, 12),
			(Row.Chart, Star)),

			Children =
		{
			createHeaderView()
				.Row(Row.Header),

			new TrendsIndicatorView(carouselPositionIndex, indicatorColor)
				.Row(Row.Indicator)
				.Center(),

			createChartView().Assign(out BaseChartView chartView)
				.Row(Row.Chart),

			createEmptyDataView()
				.Margin(chartView.Margin)
				.Padding(new Thickness(chartView.Padding.Left + 4, chartView.Padding.Top + 4, chartView.Padding.Right + 4, chartView.Padding.Bottom + 4))
				.Row(Row.Chart),

			new TrendsChartActivityIndicator(trendsPageType, deviceInfo)
				.Row(Row.Chart)
		}
		}.Padding(0, 16);

	protected enum TrendsPageType { ViewsClonesTrendsPage, StarsTrendsPage }

	enum Row { Header, Indicator, Chart }

	protected IAnalyticsService AnalyticsService { get; } = analyticsService;

	sealed class TrendsIndicatorView : IndicatorView
	{
		public TrendsIndicatorView(in int position, in Color indicatorColor)
		{
			Position = position;

			IndicatorSize = 8;

			IsEnabled = false;

			InputTransparent = true;

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
				.Bind(IsVisibleProperty,
					isFetchingDataPath,
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)))
				.Bind(IsRunningProperty,
					isFetchingDataPath,
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(TrendsViewModel)));
		}
	}
}