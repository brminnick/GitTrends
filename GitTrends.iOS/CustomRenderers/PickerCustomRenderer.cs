using GitTrends.iOS;
using GitTrends.Mobile.Common;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Picker), typeof(PickerCustomRenderer))]
namespace GitTrends.iOS
{
    public class PickerCustomRenderer : PickerRenderer
    {
        public PickerCustomRenderer() => ThemeService.PreferenceChanged += HandlePreferenceChanged;

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.TextAlignment = UITextAlignment.Center;
                Control.Layer.BorderWidth = 1;
                Control.Layer.CornerRadius = 5;

                SetBorderColor();
            }
        }

        void SetBorderColor()
        {
            if (Control?.Layer != null)
            {
                var borderColor = (Color)Xamarin.Forms.Application.Current.Resources[nameof(BaseTheme.PickerBorderColor)];
                Control.Layer.BorderColor = borderColor.ToCGColor();
            }
        }

        void HandlePreferenceChanged(object sender, PreferredTheme e) => SetBorderColor();
    }
}
