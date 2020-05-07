using Android.Content;
using GitTrends.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

//workaround for this issue: https://github.com/xamarin/Xamarin.Forms/issues/8626 and https://github.com/xamarin/Xamarin.Forms/issues/8986
[assembly: ExportRenderer(typeof(Label), typeof(LabelCustomRenderer))]
namespace GitTrends.Droid
{
    class LabelCustomRenderer : Xamarin.Forms.Platform.Android.FastRenderers.LabelRenderer
    {
        public LabelCustomRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.VerticalScrollBarEnabled = false;
            }
        }
    }
}