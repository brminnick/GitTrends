using Xamarin.Forms;

namespace GitTrends
{
	public class LabelShadowEffect : RoutingEffect
	{
		const string _id = nameof(GitTrends) + "." + nameof(LabelShadowEffect);

		public LabelShadowEffect(float radius, Color color, float distanceX, float distanceY) : base(_id)
		{
			Radius = radius;
			Color = color;
			DistanceX = distanceX;
			DistanceY = distanceY;
		}

		public float Radius { get; }
		public Color Color { get; }
		public float DistanceX { get; }
		public float DistanceY { get; }
	}
}