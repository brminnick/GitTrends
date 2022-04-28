using System;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
	abstract class BaseTrendsContentPage : BaseContentPage
	{
		protected BaseTrendsContentPage(in Color indicatorColor,
										in IMainThread mainThread,
										in int carouselPositionIndex,
										in TrendsPageType trendsPageType,
										in IAnalyticsService analyticsService) : base(analyticsService, mainThread, true)
		{
			Content = new Grid
			{
				ColumnSpacing = 8,
				RowSpacing = 12,
				Padding = new Thickness(0, 16),

				RowDefinitions = Rows.Define(
					(Row.Header, ViewsClonesStatisticsGrid.StatisticsGridHeight),
					(Row.Indicator, 12),
					(Row.Chart, Star)),

				Children =
				{
					CreateHeaderView()
						.Row(Row.Header),

					new TrendsIndicatorView(carouselPositionIndex, indicatorColor).FillExpand()
						.Row(Row.Indicator),

					CreateChartView().Assign(out BaseChartView chartView)
						.Row(Row.Chart),

					CreateEmptyDataView().Margin(chartView.Margin).Padding(new Thickness(chartView.Padding.Left + 4, chartView.Padding.Top + 4, chartView.Padding.Right + 4, chartView.Padding.Bottom + 4))
						.Row(Row.Chart),

					new TrendsChartActivityIndicator(trendsPageType)
						.Row(Row.Chart),
				}
			};
		}

		protected enum Row { Header, Indicator, Chart }
		protected enum TrendsPageType { ViewsClonesTrendsPage, StarsTrendsPage }

		protected abstract Layout CreateHeaderView();
		protected abstract BaseChartView CreateChartView();
		protected abstract EmptyDataView CreateEmptyDataView();

		class TrendsIndicatorView : IndicatorView
		{
			public TrendsIndicatorView(in int position, in Color indicatorColor)
			{
				Position = position;

				IndicatorSize = 8;

				IsEnabled = false;

				SelectedIndicatorColor = indicatorColor;
				IndicatorColor = Color.FromHex("#BFBFBF");
				AutomationId = TrendsPageAutomationIds.IndicatorView;


				this.Center();


				SetBinding(CountProperty, new Binding(nameof(TrendsCarouselPage.PageCount),
														source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(TrendsCarouselPage))));
			}
		}

		class TrendsChartActivityIndicator : ActivityIndicator
		{
			public TrendsChartActivityIndicator(TrendsPageType trendsPageType)
			{
				//The size of UIActivityIndicator is fixed by iOS, so we'll use Xamarin.Forms.VisualElement.Scale to increase its size
				//https://stackoverflow.com/a/2638224/5953643
				if (Device.RuntimePlatform is Device.iOS)
					Scale = 2;

				AutomationId = TrendsPageAutomationIds.ActivityIndicator;

				string isFetchingDataPath = trendsPageType switch
				{
					TrendsPageType.StarsTrendsPage => nameof(TrendsViewModel.IsFetchingStarsData),
					TrendsPageType.ViewsClonesTrendsPage => nameof(TrendsViewModel.IsFetchingViewsClonesData),
					_ => throw new NotImplementedException()
				};					

				this.CenterExpand()
					.DynamicResource(ColorProperty, nameof(BaseTheme.ActivityIndicatorColor))
					.Bind(IsVisibleProperty, isFetchingDataPath)
					.Bind(IsRunningProperty, isFetchingDataPath);
			}
		}
	}
}