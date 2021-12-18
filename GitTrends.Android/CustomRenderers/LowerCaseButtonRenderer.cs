using Android.Content;
using GitTrends.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Button), typeof(LowerCaseButtonRenderer))]
namespace GitTrends.Droid;

public class LowerCaseButtonRenderer : ButtonRenderer
{
	public LowerCaseButtonRenderer(Context context) : base(context)
	{

	}

	protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
	{
		base.OnElementChanged(e);

		if (Control != null)
		{
			Control.SetAllCaps(false);
			Control.VerticalScrollBarEnabled = false;
		}
	}
}
