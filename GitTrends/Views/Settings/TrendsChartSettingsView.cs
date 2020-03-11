using System.Linq;
using GitTrends.Mobile.Shared;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class TrendsChartSettingsView : ContentView
    {
        public TrendsChartSettingsView(in TrendsChartSettingsService trendsChartSettingsService)
        {
            Content = new Grid
            {
                RowSpacing = 2,

                RowDefinitions = Rows.Define(
                    (Row.Label, new GridLength(1, GridUnitType.Star)),
                    (Row.Control, new GridLength(2, GridUnitType.Star))),

                ColumnDefinitions = Columns.Define(new GridLength(1, GridUnitType.Star)),

                Children =
                {
                    new TrendsChartSettingsLabel().Row(Row.Label),
                    new TrendsCharSettingsControl(trendsChartSettingsService).Row(Row.Control)
                }
            };
        }

        enum Row { Label, Control }

        class TrendsChartSettingsLabel : Label
        {
            public TrendsChartSettingsLabel()
            {
                AutomationId = SettingsPageAutomationIds.TrendsChartSettingsLabel;
                Text = "Preferred Charts";
                FontAttributes = FontAttributes.Bold;
                FontSize = 18;
                VerticalTextAlignment = TextAlignment.Start;
                SetDynamicResource(TextColorProperty, nameof(BaseTheme.TrendsChartSettingsLabelTextColor));
            }
        }

        class TrendsCharSettingsControl : SfSegmentedControl
        {
            const double cornerRadius = 5;
            readonly TrendsChartSettingsService _trendsChartSettingsService;

            public TrendsCharSettingsControl(in TrendsChartSettingsService trendsChartSettingsService)
            {
                _trendsChartSettingsService = trendsChartSettingsService;

                AutomationId = SettingsPageAutomationIds.TrendsChartSettingsControl;
                ItemsSource = TrendsChartConstants.TrendsChartTitles.Values.ToList();
                VisibleSegmentsCount = TrendsChartConstants.TrendsChartTitles.Values.Count;
                CornerRadius = cornerRadius;
                SelectedIndex = (int)trendsChartSettingsService.CurrentTrendsChartOption;
                SelectionIndicatorSettings = new TrendsChartSettingsSelectionIndicatorSettings();

                SetDynamicResource(BorderColorProperty, nameof(BaseTheme.TrendsChartSettingsBorderColor));
                SetDynamicResource(FontColorProperty, nameof(BaseTheme.TrendsChartSettingsFontColor));

                SelectionChanged += HandleSelectionChanged;
            }


            void HandleSelectionChanged(object sender, Syncfusion.XForms.Buttons.SelectionChangedEventArgs e) =>
                _trendsChartSettingsService.CurrentTrendsChartOption = (TrendsChartOption)e.Index;

            class TrendsChartSettingsSelectionIndicatorSettings : SelectionIndicatorSettings
            {
                public TrendsChartSettingsSelectionIndicatorSettings()
                {
                    CornerRadius = cornerRadius;
                    this.SetDynamicResource(ColorProperty, nameof(BaseTheme.TrendsChartSettingsSelectionIndicatorColor));
                }
            }
        }
    }
}
