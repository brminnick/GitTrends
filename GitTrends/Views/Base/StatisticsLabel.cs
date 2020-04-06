using System;
using Xamarin.Forms;

namespace GitTrends.Views.Base
{
    class StatisticsLabel : Label
    {
        public StatisticsLabel(in double fontSize, in long number, in string textColorThemeName, in string fontFamilyName)
        {
            Text = GetNumberAsText(number);
            FontSize = fontSize;

            HorizontalOptions = LayoutOptions.FillAndExpand;
            MaxLines = 1;
            HorizontalTextAlignment = TextAlignment.Start;
            VerticalTextAlignment = TextAlignment.End;
            LineBreakMode = LineBreakMode.TailTruncation;

            SetDynamicResource(TextColorProperty, textColorThemeName);
            SetDynamicResource(FontFamilyProperty, nameof(fontFamilyName));
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
