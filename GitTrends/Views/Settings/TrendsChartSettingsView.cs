using System.Linq;
using System.Collections.Generic;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;
using GitTrends.Mobile.Shared;

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
                AutomationId = SettingsPageAutomationIds.TrendsChartSettingsLabel,
                Text = "Preferred Charts",
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                VerticalTextAlignment = TextAlignment.Start
            };
            trendsChartSettingsLabel.SetDynamicResource(Label.TextColorProperty, nameof(BaseTheme.TrendsChartSettingsLabelTextColor));

            var selectionIndicatorSettings = new SelectionIndicatorSettings
            {
                CornerRadius = cornerRadius
            };
            selectionIndicatorSettings.SetDynamicResource(SelectionIndicatorSettings.ColorProperty, nameof(BaseTheme.TrendsChartSettingsSelectionIndicatorColor));

            var trendsChartSettingControl = new SfSegmentedControl
            {
                AutomationId = SettingsPageAutomationIds.TrendsChartSettingsControl,
                ItemsSource = _trendsChartOptions.Values.ToList(),
                VisibleSegmentsCount = _trendsChartOptions.Values.Count,
                CornerRadius = cornerRadius,
                SelectedIndex = (int)_trendsChartSettingsService.CurrentTrendsChartOption,
                SelectionIndicatorSettings = selectionIndicatorSettings
            };
            trendsChartSettingControl.SetDynamicResource(SfSegmentedControl.BorderColorProperty, nameof(BaseTheme.TrendsChartSettingsBorderColor));
            trendsChartSettingControl.SetDynamicResource(SfSegmentedControl.FontColorProperty, nameof(BaseTheme.TrendsChartSettingsFontColor));
            trendsChartSettingControl.SelectionChanged += HandleSelectionChanged;

            var grid = new Grid
            {
                RowSpacing = 2,
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
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
