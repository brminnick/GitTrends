using Xamarin.Forms;

namespace GitTrends
{
    class StatisticsLabel : Label
    {
        public StatisticsLabel(in double fontSize, in long number, in string textColorThemeName, in string fontFamily)
        {
            Text = GetNumberAsText(number);
            FontSize = fontSize;

            HorizontalOptions = LayoutOptions.FillAndExpand;
            MaxLines = 1;
            HorizontalTextAlignment = TextAlignment.Start;
            VerticalTextAlignment = TextAlignment.End;
            LineBreakMode = LineBreakMode.TailTruncation;
            FontFamily = fontFamily;

            SetDynamicResource(TextColorProperty, textColorThemeName);
        }

        static string GetNumberAsText(long number)
        {
            if (number < 10e2)
                return string.Format("{0:0}", number);
            else if (number < 10e5)
                return $"{string.Format("{0:0.0}", number / 10e2)}K";
            else if (number < 10e8)
                return $"{string.Format("{0:0.0}", number / 10e5)}M";
            else if (number < 10e11)
                return $"{string.Format("{0:0.0}", number / 10e8)}B";
            else if (number < 10e14)
                return $"{string.Format("{0:0.0}", number / 10e11)}T";

            return "0";
        }
    }
}
