using Xamarin.Forms;

namespace GitTrends
{
    class BorderButton : BounceButton
    {
        public BorderButton(in string automationId)
        {
            AutomationId = automationId;

            FontSize = 14;
            FontFamily = FontFamilyConstants.RobotoMedium;

            Padding = new Thickness(10, 0);
            BorderWidth = Device.RuntimePlatform is Device.Android ? 0.75 : 1;

            //Use the default corner radius on iOS
            CornerRadius = Device.RuntimePlatform is Device.Android ? 7 : -1;


            this.DynamicResources((TextColorProperty, nameof(BaseTheme.BorderButtonFontColor)),
                                    (BorderColorProperty, nameof(BaseTheme.BorderButtonBorderColor)),
                                    (BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor)));
        }
    }
}
