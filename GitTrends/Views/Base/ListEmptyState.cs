using System;
using Xamarin.Forms;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;
using Xamarin.Forms.Markup;

namespace GitTrends.Views.Base
{
    public class ListEmptyState : Grid
    {
        public ListEmptyState(in string imageSource, in double width, in double height, in string text, in bool isChartEmptyState = false)
        {

            RowSpacing = isChartEmptyState ? 48 : 72;

            RowDefinitions = Rows.Define(
                (EmptyStateRows.Title, isChartEmptyState ? StarGridLength(1) : StarGridLength(3)),
                (EmptyStateRows.EmptyStateImage, isChartEmptyState ? StarGridLength(9) : StarGridLength(7)));

            ColumnDefinitions = Columns.Define(
                (EmptyStateColumns.Content, StarGridLength(1)));

            Children.Add(new TitleLabel(text)
                            .Row(EmptyStateRows.Title).Column(EmptyStateColumns.Content));

            Children.Add(new EmptyStateImage(imageSource, width, height)
                            .Row(EmptyStateRows.EmptyStateImage).Column(EmptyStateColumns.Content));

            if (isChartEmptyState)
                this.SetBinding(IsVisibleProperty, nameof(TrendsViewModel.IsEmptyStateVisible), BindingMode.OneWay);
        }
    }

    enum EmptyStateRows { Title, EmptyStateImage}
    enum EmptyStateColumns { Content }

    public class TitleLabel : Label
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

    public class EmptyStateImage : Image
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
