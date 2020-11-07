using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static Xamarin.Forms.Markup.GridRowsColumns;
using static GitTrends.MarkupExtensions;

namespace GitTrends
{
    class StarsStatisticsGrid : Grid
    {
        public StarsStatisticsGrid()
        {
            this.FillExpand()
                .DynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardStarsStatsIconColor));

            Padding = 20;
            RowSpacing = 8;

            RowDefinitions = Rows.Define(
                (Row.TopLine, AbsoluteGridLength(1)),
                (Row.Total, StarGridLength(1)),
                (Row.Stars, StarGridLength(3)),
                (Row.Message, StarGridLength(1)),
                (Row.BottomLine, AbsoluteGridLength(1)));

            ColumnDefinitions = Columns.Define(
                (Column.LeftStar, StarGridLength(1)),
                (Column.Text, StarGridLength(1)),
                (Column.RightStar, StarGridLength(1)));

            Children.Add(new SeparatorLine()
                            .Row(Row.TopLine).ColumnSpan(All<Column>()));

            Children.Add(new StarsStatisticsLabel("Total", 24)
                            .Row(Row.Total).ColumnSpan(All<Column>()));

            Children.Add(new StarSvg()
                            .Row(Row.Stars).Column(Column.LeftStar));

            Children.Add(new StarsStatisticsLabel(48)
                            .Row(Row.Stars).Column(Column.Text)
                            .Bind(Label.TextProperty, nameof(TrendsViewModel.TotalStars)));

            Children.Add(new StarSvg()
                            .Row(Row.Stars).Column(Column.RightStar));

            Children.Add(new StarsStatisticsLabel("KEEP IT UP!", 24)
                            .Row(Row.Message).ColumnSpan(All<Column>()));

            Children.Add(new SeparatorLine()
                            .Row(Row.BottomLine).ColumnSpan(All<Column>()));
        }

        enum Row { TopLine, Total, Stars, Message, BottomLine }
        enum Column { LeftStar, Text, RightStar }

        class StarSvg : SvgImage
        {
            public StarSvg()
                : base("star.svg", () => (Color)Application.Current.Resources[nameof(BaseTheme.PageBackgroundColor)], 44, 44)
            {
                this.Center();
            }
        }

        class SeparatorLine : BoxView
        {
            public SeparatorLine() => SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
        }

        class StarsStatisticsLabel : Label
        {
            public StarsStatisticsLabel(in string text, in int fontSize) : this(fontSize)
            {
                Text = text;
            }

            public StarsStatisticsLabel(in int fontSize)
            {
                this.TextCenter()
                    .DynamicResource(TextColorProperty, nameof(BaseTheme.PageBackgroundColor));
                FontSize = fontSize;
                FontFamily = FontFamilyConstants.RobotoBold;
            }
        }
    }
}
