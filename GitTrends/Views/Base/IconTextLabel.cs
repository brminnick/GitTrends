using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;

namespace GitTrends
{
    class SvgTextLabel : PancakeView
    {
        public SvgTextLabel(in string svgFileName, in string text, in string automationId, in int fontSize, in string fontFamily, in double logoTextSpacing)
        {
            AutomationId = automationId;

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

            this.CenterExpand().Padding(16, 10);
        }

        class TextLabel : Label
        {
            public TextLabel(in string text, in int fontSize = 18, in string fontFamily = FontFamilyConstants.RobotoRegular)
            {
                Text = text;
                TextColor = Color.White;

                LineBreakMode = LineBreakMode.TailTruncation;

                this.FillExpand().TextCenter().Font(fontFamily, fontSize);
            }
        }
    }
}
