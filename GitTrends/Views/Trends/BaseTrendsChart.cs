using Syncfusion.SfChart.XForms;
using Xamarin.Forms;

namespace GitTrends
{
    abstract class BaseTrendsChart : SfChart
    {
        protected BaseTrendsChart(in string automationId)
        {
            Margin = 0;
            ChartPadding = new Thickness(0, 24, 0, 4);

            BackgroundColor = Color.Transparent;

            AutomationId = automationId;

            ChartBehaviors = new ChartBehaviorCollection
            {
                new ChartZoomPanBehavior(),
                new ChartTrackballBehavior()
            };
        }

        protected class TrendsAreaSeries : AreaSeries
        {
            public TrendsAreaSeries(in string title, in string xDataTitle, in string yDataTitle, in string colorResource)
            {
                Opacity = 0.9;
                Label = title;
                XBindingPath = xDataTitle;
                YBindingPath = yDataTitle;
                LegendIcon = ChartLegendIcon.SeriesType;

                this.DynamicResource(ColorProperty, colorResource);
            }
        }
    }
}
