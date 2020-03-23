using GitTrends.Mobile.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class RegisterForNotificationsView : ContentView
    {
        public RegisterForNotificationsView()
        {
            Content = new Grid
            {
                RowDefinitions = Rows.Define(AbsoluteGridLength(40)),

                ColumnDefinitions = Columns.Define(
                    (Column.Label, StarGridLength(2)),
                    (Column.Button, StarGridLength(1))),

                Children =
                {
                    new RegisterForNotificationsLabel().Column(Column.Label),
                    new RegisterForNotificationsButton().Column(Column.Button),
                }
            };
        }

        enum Column { Label, Button }

        class RegisterForNotificationsLabel : SettingsLabel
        {
            public RegisterForNotificationsLabel() : base("Register For Notifications", SettingsPageAutomationIds.RegisterForNotificationsLabel)
            {
                VerticalTextAlignment = TextAlignment.Center;
            }
        }

        class RegisterForNotificationsButton : SettingsButton
        {
            public RegisterForNotificationsButton() : base("Register", SettingsPageAutomationIds.RegisterForNotificationsButton)
            {
                HorizontalOptions = LayoutOptions.End;

                this.SetBinding(CommandProperty, nameof(SettingsViewModel.RegisterForPushNotificationsButtonCommand));
            }
        }
    }
}
