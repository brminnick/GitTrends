using System;
using System.Linq;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Syncfusion.XForms.Buttons;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.MarkupExtensions;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class PreferredChartsView : Grid
    {
        public PreferredChartsView(SettingsViewModel settingsViewModel, IMainThread mainThread)
        {
            RowSpacing = 2;

            VerticalOptions = LayoutOptions.End;

            RowDefinitions = Rows.Define(
                (Row.Label, StarGridLength(1)),
                (Row.Control, StarGridLength(2)));

            Children.Add(new PreferredChartSettingsLabel().Row(Row.Label));
            Children.Add(new PreferredChartSettingsControl(settingsViewModel, mainThread).Row(Row.Control));
        }

        enum Row { Label, Control }

        class PreferredChartSettingsLabel : TitleLabel
        {
            public PreferredChartSettingsLabel()
            {
                VerticalTextAlignment = TextAlignment.Start;
                AutomationId = SettingsPageAutomationIds.PreferredChartSettingsLabel;
                this.SetBinding(TextProperty, nameof(SettingsViewModel.PreferredChartsLabelText));
            }
        }

        class PreferredChartSettingsControl : SfSegmentedControl
        {
            const double cornerRadius = 4;
            readonly SettingsViewModel _settingsViewModel;
            readonly IMainThread _mainThread;

            public PreferredChartSettingsControl(in SettingsViewModel settingsViewModel, in IMainThread mainThread)
            {
                LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

                _mainThread = mainThread;
                _settingsViewModel = settingsViewModel;

                FontSize = 12;
                FontFamily = FontFamilyConstants.RobotoMedium;

                CornerRadius = cornerRadius;
                AutomationId = SettingsPageAutomationIds.PreferredChartSettingsControl;

                VisibleSegmentsCount = TrendsChartConstants.TrendsChartTitles.Values.Count;

                SelectedIndex = _settingsViewModel.PreferredChartsSelectedIndex;
                SelectionIndicatorSettings = new TrendsChartSettingsSelectionIndicatorSettings();

                SetItemSource();

                this.DynamicResources((FontColorProperty, nameof(BaseTheme.BorderButtonFontColor)),
                                        (BorderColorProperty, nameof(BaseTheme.BorderButtonBorderColor)),
                                        (BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor)))
                    .Bind(SelectedIndexProperty, nameof(SettingsViewModel.PreferredChartsSelectedIndex));
            }

            void HandlePreferredLanguageChanged(object sender, string? e) => _mainThread.BeginInvokeOnMainThread(SetItemSource);

            void SetItemSource() => ItemsSource = TrendsChartConstants.TrendsChartTitles.Values.ToList();

            class TrendsChartSettingsSelectionIndicatorSettings : SelectionIndicatorSettings
            {
                public TrendsChartSettingsSelectionIndicatorSettings()
                {
                    CornerRadius = cornerRadius;
                    this.DynamicResource(ColorProperty, nameof(BaseTheme.TrendsChartSettingsSelectionIndicatorColor));
                }
            }
        }
    }
}
