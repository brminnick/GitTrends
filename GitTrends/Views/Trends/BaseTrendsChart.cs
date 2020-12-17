using System.Threading.Tasks;
using Syncfusion.SfChart.XForms;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    abstract class BaseTrendsChart : SfChart
    {
        readonly IMainThread _mainThread;
        readonly ChartZoomPanBehavior _chartZoomPanBehavior = new();

        protected BaseTrendsChart(in IMainThread mainThread, in string automationId)
        {
            _mainThread = mainThread;
            AutomationId = automationId;

            Margin = 0;
            ChartPadding = new Thickness(0, 24, 0, 4);

            BackgroundColor = Color.Transparent;

            ChartBehaviors = new ChartBehaviorCollection
            {
                _chartZoomPanBehavior,
                new ChartTrackballBehavior()
            };
        }

        protected Task SetZoom(double primaryAxisStart, double primaryAxisEnd, double secondaryAxisStart, double secondaryAxisEnd) => _mainThread.InvokeOnMainThreadAsync(() =>
        {
            _chartZoomPanBehavior.ZoomByRange(PrimaryAxis, primaryAxisStart, primaryAxisEnd);
            _chartZoomPanBehavior.ZoomByRange(SecondaryAxis, secondaryAxisStart, secondaryAxisEnd);
        });

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
