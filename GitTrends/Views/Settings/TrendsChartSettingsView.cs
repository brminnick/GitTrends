using System.Linq;
using GitTrends.Mobile.Shared;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
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
                    (Row.Label, Star),
                    (Row.Control, StarGridLength(2))),

                ColumnDefinitions = Columns.Define(StarGridLength(1)),

                Children =
                {
                    new TrendsChartSettingsLabel().Row(Row.Label).Margin(new Thickness(16,0,0,12)),
                    new TrendsCharSettingsControl(trendsChartSettingsService).Row(Row.Control)
                }
            };
        }

        enum Row { Label, Control }

        class TrendsChartSettingsLabel : SettingsLabel
        {
            public TrendsChartSettingsLabel() : base("Preferred Charts", SettingsPageAutomationIds.TrendsChartSettingsLabel)
            {
                VerticalTextAlignment = TextAlignment.Start;
            }
        }

        class TrendsCharSettingsControl : SfSegmentedControl
        {
            const double cornerRadius = 4;
            readonly TrendsChartSettingsService _trendsChartSettingsService;

            public TrendsCharSettingsControl(in TrendsChartSettingsService trendsChartSettingsService)
            {
                _trendsChartSettingsService = trendsChartSettingsService;

                CornerRadius = cornerRadius;
                AutomationId = SettingsPageAutomationIds.TrendsChartSettingsControl;
                ItemsSource = TrendsChartConstants.TrendsChartTitles.Values.ToList();
                VisibleSegmentsCount = TrendsChartConstants.TrendsChartTitles.Values.Count;
                SelectedIndex = (int)trendsChartSettingsService.CurrentTrendsChartOption;
                SelectionIndicatorSettings = new TrendsChartSettingsSelectionIndicatorSettings();
                FontFamily = FontFamilyConstants.RobotoMedium;
                FontSize = 12;

                SetDynamicResource(FontColorProperty, nameof(BaseTheme.SettingsButtonFontColor));
                SetDynamicResource(BorderColorProperty, nameof(BaseTheme.SettingsButtonBorderColor));

                SelectionChanged += HandleSelectionChanged;
            }

            void HandleSelectionChanged(object sender, Syncfusion.XForms.Buttons.SelectionChangedEventArgs e) =>
                _trendsChartSettingsService.CurrentTrendsChartOption = (TrendsChartOption)e.Index;

            class TrendsChartSettingsSelectionIndicatorSettings : SelectionIndicatorSettings
            {
                public TrendsChartSettingsSelectionIndicatorSettings()
                {
                    CornerRadius = cornerRadius;
                    SetDynamicResource(ColorProperty, nameof(BaseTheme.TrendsChartSettingsSelectionIndicatorColor));
                }
            }
        }
    }
}
