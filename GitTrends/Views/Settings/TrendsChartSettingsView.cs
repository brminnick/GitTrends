using System.Linq;
using System.Collections.Generic;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;

namespace GitTrends
{
    public class TrendsChartSettingsView : ContentView
    {
        readonly Dictionary<TrendsChartOptions, string> _trendsChartOptions = new Dictionary<TrendsChartOptions, string>
        {
            { TrendsChartOptions.All, "All" },
            { TrendsChartOptions.NoUniques, "No Uniques" },
            { TrendsChartOptions.JustUniques, "Just Uniques" }
        };

        readonly TrendsChartSettingsService _trendsChartSettingsService;

        public TrendsChartSettingsView(TrendsChartSettingsService trendsChartSettingsService)
        {
            _trendsChartSettingsService = trendsChartSettingsService;

            const double cornerRadius = 5;

            var trendsChartSettingsLabel = new Label
            {
                Text = "Default Charts",
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                TextColor = ColorConstants.DarkNavyBlue,
                VerticalTextAlignment = TextAlignment.Center
            };

            var trendsChartSettingControl = new SfSegmentedControl
            {
                ItemsSource = _trendsChartOptions.Values.ToList(),
                VisibleSegmentsCount = _trendsChartOptions.Values.Count,
                BorderColor = ColorConstants.DarkBlue,
                CornerRadius = cornerRadius,
                SelectedIndex = (int)_trendsChartSettingsService.CurrentTrendsChartOption,
                FontColor = ColorConstants.DarkNavyBlue,
                SelectionIndicatorSettings = new SelectionIndicatorSettings
                {
                    Color = ColorConstants.DarkBlue,
                    CornerRadius = cornerRadius
                }
            };
            trendsChartSettingControl.SelectionChanged += HandleSelectionChanged;

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) }
                }
            };

            grid.Children.Add(trendsChartSettingsLabel, 0, 0);
            grid.Children.Add(trendsChartSettingControl, 0, 1);

            Content = grid;
        }

        void HandleSelectionChanged(object sender, Syncfusion.XForms.Buttons.SelectionChangedEventArgs e) =>
            _trendsChartSettingsService.CurrentTrendsChartOption = (TrendsChartOptions)e.Index;
    }
}
