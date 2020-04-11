using GitTrends.Mobile.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class RegisterForNotificationsView : Grid
    {
        public RegisterForNotificationsView()
        {
            RowSpacing = 5;

            RowDefinitions = Rows.Define(
                (Row.Label, AbsoluteGridLength(20)),
                (Row.Button, AbsoluteGridLength(40)));

            Children.Add(new RegisterForNotificationsLabel().Row(Row.Label));
            Children.Add(new RegisterForNotificationsButton().Row(Row.Button));
        }

        enum Row { Label, Button }

        class RegisterForNotificationsLabel : SettingsLabel
        {
            public RegisterForNotificationsLabel() : base("Register For Notifications", SettingsPageAutomationIds.RegisterForNotificationsLabel)
            {
                VerticalTextAlignment = TextAlignment.End;
            }
        }

        class RegisterForNotificationsButton : SettingsButton
        {
            public RegisterForNotificationsButton() : base("Register", SettingsPageAutomationIds.RegisterForNotificationsButton)
            {
                HorizontalOptions = LayoutOptions.Start;

                this.SetBinding(CommandProperty, nameof(SettingsViewModel.RegisterForPushNotificationsButtonCommand));
            }
        }
    }
}
