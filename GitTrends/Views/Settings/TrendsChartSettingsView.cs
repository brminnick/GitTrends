using System.Linq;
using GitTrends.Mobile.Shared;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class TrendsChartSettingsView : Grid
    {
        public TrendsChartSettingsView(in TrendsChartSettingsService trendsChartSettingsService)
        {
            RowSpacing = 2;

            RowDefinitions = Rows.Define(
                (Row.Label, StarGridLength(1)),
                (Row.Control, StarGridLength(2)));

            Children.Add(new TrendsChartSettingsLabel().Row(Row.Label).Margin(new Thickness(16, 0, 0, 12)));
            Children.Add(new TrendsCharSettingsControl(trendsChartSettingsService).Row(Row.Control));
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
                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));

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
