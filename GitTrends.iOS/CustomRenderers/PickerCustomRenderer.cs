using System.ComponentModel;
using GitTrends.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Picker), typeof(PickerCustomRenderer))]
namespace GitTrends.iOS
{
    public class PickerCustomRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
                Control.TextAlignment = UITextAlignment.Center;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Control != null)
            {
                var borderColor = (Color)Xamarin.Forms.Application.Current.Resources[nameof(BaseTheme.SettingsButtonBorderColor)];
                Control.Layer.BorderColor = borderColor.ToCGColor();
            }
        }
    }
}
