using Android.Content;
using GitTrends.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Picker), typeof(PickerCustomRenderer))]
namespace GitTrends.Droid
{
    public class PickerCustomRenderer : PickerRenderer
    {
        public PickerCustomRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
                Control.Gravity = Android.Views.GravityFlags.Center;
        }
    }
}
