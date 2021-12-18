using System.Linq;
using CoreGraphics;
using GitTrends;
using GitTrends.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName(nameof(GitTrends))]
[assembly: ExportEffect(typeof(LabelShadowEffect_iOS), nameof(LabelShadowEffect))]
namespace GitTrends.iOS;

public class LabelShadowEffect_iOS : PlatformEffect
{
	protected override void OnAttached()
	{
		var effect = Element.Effects.OfType<LabelShadowEffect>().First();

		Control.Layer.ShadowRadius = effect.Radius;
		Control.Layer.ShadowColor = effect.Color.ToCGColor();
		Control.Layer.ShadowOffset = new CGSize(effect.DistanceX, effect.DistanceY);
		Control.Layer.ShadowOpacity = 1.0f;
	}

	protected override void OnDetached()
	{
		Control.Layer.ShadowRadius = 0;
		Control.Layer.ShadowColor = Color.Transparent.ToCGColor();
		Control.Layer.ShadowOffset = CGSize.Empty;
		Control.Layer.ShadowOpacity = 0;
	}
}
