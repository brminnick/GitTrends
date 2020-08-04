using Xamarin.Forms;

namespace GitTrends
{
    class StatisticsLabel : Label
    {
        public const int StatisticsFontSize = 12;

        public StatisticsLabel(in string text, in bool isVisible, in string textColorThemeName)
        {
            Text = text;
            FontSize = StatisticsFontSize;

            IsVisible = isVisible;

            HorizontalOptions = LayoutOptions.FillAndExpand;

            HorizontalTextAlignment = TextAlignment.Start;
            VerticalTextAlignment = TextAlignment.End;

            LineBreakMode = LineBreakMode.TailTruncation;

            this.DynamicResource(TextColorProperty, textColorThemeName);
        }
    }
}
