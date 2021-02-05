using Xamarin.Forms;
using Xamarin.Forms.PancakeView;

namespace GitTrends
{
    class SvgTextLabel : PancakeView
    {
        public SvgTextLabel(in string svgFileName, in string text, in string automationId, in int fontSize, in string fontFamily, in double logoTextSpacing)
        {
            AutomationId = automationId;
            HorizontalOptions = LayoutOptions.CenterAndExpand;
            VerticalOptions = LayoutOptions.CenterAndExpand;
            Padding = new Thickness(16, 10);
            CornerRadius = 4;

            Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = logoTextSpacing,
                Children =
                {
                    new SvgImage(svgFileName, () => Color.White),
                    new TextLabel(text, fontSize,fontFamily)
                }
            };
        }

        class TextLabel : Label
        {
            public TextLabel(in string text, in int fontSize = 18, in string fontFamily = FontFamilyConstants.RobotoRegular)
            {
                Text = text;
                FontSize = fontSize;
                FontFamily = fontFamily;

                LineBreakMode = LineBreakMode.TailTruncation;

                HorizontalTextAlignment = TextAlignment.Center;
                VerticalTextAlignment = TextAlignment.Center;
                VerticalOptions = LayoutOptions.CenterAndExpand;
                TextColor = Color.White;
            }
        }
    }
}
