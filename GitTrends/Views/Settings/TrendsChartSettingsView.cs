using Xamarin.Forms;

namespace GitTrends
{
    public class TrendsChartSettingsView : ContentView
    {
        public TrendsChartSettingsView()
        {
            var trendsChartSettingsLabel = new TrendsChartSettingsLabel("Trends Chart Settings")
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 18
            };

            var shouldShowClonesByDefaultLabel = new TrendsChartSettingsLabel("Show Clones");

            var shouldShowClonesByDefaultSwitch = new TrendsChartSettingsSwitch();
            shouldShowClonesByDefaultSwitch.SetBinding(Switch.IsToggledProperty, nameof(SettingsViewModel.ShouldShowClonesByDefaultSwitchValue));

            var shouldShowUniqueClonesByDefaultLabel = new TrendsChartSettingsLabel("Show Unique Clones");

            var shouldShowUniqueClonesByDefaultSwitch = new TrendsChartSettingsSwitch();
            shouldShowUniqueClonesByDefaultSwitch.SetBinding(Switch.IsToggledProperty, nameof(SettingsViewModel.ShouldShowUniqueClonesByDefaultSwitchValue));

            var shouldShowViewsByDefaultLabel = new TrendsChartSettingsLabel("Show Views");

            var shouldShowViewsByDefaultSwitch = new TrendsChartSettingsSwitch();
            shouldShowViewsByDefaultSwitch.SetBinding(Switch.IsToggledProperty, nameof(SettingsViewModel.ShouldShowViewsByDefaultSwitchValue));

            var shouldShowUniqueViewsByDefaultLabel = new TrendsChartSettingsLabel("Show Unique Views");

            var shouldShowUniqueViewsByDefaultSwitch = new TrendsChartSettingsSwitch();
            shouldShowUniqueViewsByDefaultSwitch.SetBinding(Switch.IsToggledProperty, nameof(SettingsViewModel.ShouldShowUniqueViewsByDefaultSwitchValue));

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };

            grid.Children.Add(trendsChartSettingsLabel, 0, 0);
            Grid.SetColumnSpan(trendsChartSettingsLabel, 2);

            grid.Children.Add(shouldShowClonesByDefaultLabel, 0, 1);
            grid.Children.Add(shouldShowClonesByDefaultSwitch, 1, 1);

            grid.Children.Add(shouldShowViewsByDefaultLabel, 0, 2);
            grid.Children.Add(shouldShowViewsByDefaultSwitch, 1, 2);

            grid.Children.Add(shouldShowUniqueClonesByDefaultLabel, 0, 3);
            grid.Children.Add(shouldShowUniqueClonesByDefaultSwitch, 1, 3);

            grid.Children.Add(shouldShowUniqueViewsByDefaultLabel, 0, 4);
            grid.Children.Add(shouldShowUniqueViewsByDefaultSwitch, 1, 4);

            Content = grid;
        }

        class TrendsChartSettingsSwitch : Switch
        {
            public TrendsChartSettingsSwitch()
            {
                ThumbColor = Color.White;
                OnColor = ColorConstants.DarkestBlue;
                HorizontalOptions = LayoutOptions.End;
            }
        }

        class TrendsChartSettingsLabel : Label
        {
            public TrendsChartSettingsLabel(string text)
            {
                Text = text;
                TextColor = ColorConstants.DarkNavyBlue;
                VerticalTextAlignment = TextAlignment.Center;
            }
        }
    }
}
