using Android.Content;
using Android.Graphics.Drawables;
using GitTrends.Droid;
using GitTrends.Mobile.Common;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Picker), typeof(PickerCustomRenderer))]
namespace GitTrends.Droid
{
    public class PickerCustomRenderer : PickerRenderer
    {
        public PickerCustomRenderer(Context context) : base(context) => ThemeService.PreferenceChanged += HandlePreferenceChanged;

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Background = null;
                Control.Gravity = Android.Views.GravityFlags.Center;
                Control.VerticalScrollBarEnabled = false;

                SetPickerBorder();
            }
        }

        void SetPickerBorder()
        {
            if (Application.Current?.Resources is BaseTheme theme)
            {
                var borderColor = theme.PickerBorderColor;

                var gradientDrawable = new GradientDrawable();
                gradientDrawable.SetCornerRadius(10);
                gradientDrawable.SetStroke(2, borderColor.ToAndroid());

                if (Control != null)
                {
                    try
                    {
                        Control.Background = gradientDrawable;
                    }
                    catch (System.ObjectDisposedException)
                    {

                    }
                }
            }
        }

        void HandlePreferenceChanged(object sender, PreferredTheme e) => SetPickerBorder();
    }
}
