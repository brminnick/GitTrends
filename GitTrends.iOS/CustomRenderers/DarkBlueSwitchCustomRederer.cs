using GitTrends.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Switch), typeof(DarkBlueSwitchCustomRederer))]
namespace GitTrends.iOS
{
    public class DarkBlueSwitchCustomRederer : SwitchRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                var buttonBackgroundColor = (Color)Xamarin.Forms.Application.Current.Resources[nameof(BaseTheme.ButtonBackgroundColor)];
                Control.TintColor = buttonBackgroundColor.MultiplyAlpha(0.25).ToUIColor();
            }
        }
    }
}
