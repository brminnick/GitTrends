using System;
using System.Linq;
using GitTrends.Droid;
using GitTrends;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName(nameof(GitTrends))]
[assembly: ExportEffect(typeof(LabelShadowEffect_Android), nameof(LabelShadowEffect))]
namespace GitTrends.Droid
{
    public class LabelShadowEffect_Android : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                var control = (Android.Widget.TextView)Control;
                var effect = Element.Effects.OfType<LabelShadowEffect>().First();

                control.SetShadowLayer(effect.Radius, effect.DistanceX, effect.DistanceY, effect.Color.ToAndroid());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
            }
        }

        protected override void OnDetached()
        {
            var control = (Android.Widget.TextView)Control;
            control.SetShadowLayer(0, 0, 0, Color.Transparent.ToAndroid());
        }
    }
}
