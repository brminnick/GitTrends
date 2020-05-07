using Android.Content;
using Android.Graphics;
using GitTrends;
using GitTrends.Droid;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FontAwesomeButton), typeof(FontAwesomeIconRenderer))]
namespace GitTrends.Droid
{
    public class FontAwesomeIconRenderer : ButtonRenderer
    {
        public FontAwesomeIconRenderer(Context context) : base(context)
        {
        }

        Context CurrentContext => CrossCurrentActivity.Current.AppContext;

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement is null && Control is Android.Widget.Button button)
                button.Typeface = Typeface.CreateFromAsset(CurrentContext.Assets, "FontAwesomeBrands.ttf");
        }
    }
}
