using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;

namespace GitTrends
{
	class StatisticsLabel : Label
	{
		public const int StatisticsFontSize = 12;

		public StatisticsLabel(in string textColorThemeName)
		{
			FontSize = StatisticsFontSize;

			HorizontalOptions = LayoutOptions.FillAndExpand;

			HorizontalTextAlignment = TextAlignment.Start;
			VerticalTextAlignment = TextAlignment.End;

			LineBreakMode = LineBreakMode.TailTruncation;

			this.DynamicResource(TextColorProperty, textColorThemeName);
		}
	}
}