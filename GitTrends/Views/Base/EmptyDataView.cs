using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends.Views.Base
{
    class EmptyDataView : Grid
    {
        public EmptyDataView(in string imageSource, in string text, in double rowSpacing = 72)
        {
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Start;

            RowSpacing = rowSpacing;

            RowDefinitions = Rows.Define(
                (Row.Title, StarGridLength(3)),
                (Row.Image, StarGridLength(7)));

            Children.Add(new TitleLabel(text).Row(Row.Title));
            Children.Add(new EmptyStateImage(imageSource, 250, 250).Row(Row.Image));
        }

        enum Row { Title, Image }

        class TitleLabel : Label
        {
            public TitleLabel(in string text)
            {
                Text = text;
                FontSize = 24;
                FontFamily = FontFamilyConstants.RobotoMedium;
                HorizontalTextAlignment = TextAlignment.Center;
                VerticalTextAlignment = TextAlignment.End;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
            }
        }

        class EmptyStateImage : Image
        {
            public EmptyStateImage(in string source, in double width, in double height)
            {
                Source = source;
                HorizontalOptions = LayoutOptions.FillAndExpand;
                VerticalOptions = LayoutOptions.StartAndExpand;
                WidthRequest = width;
                HeightRequest = height;
            }
        }
    }
}
