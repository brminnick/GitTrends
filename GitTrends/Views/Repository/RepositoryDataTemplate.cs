using FFImageLoading.Svg.Forms;
using GitTrends.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class RepositoryDataTemplate : DataTemplate
    {
        const int circleImageHeight = 90;
        const int emojiColumnSize = 15;
        const int statisticColumnSize = 30;

        const int _smallFontSize = 12;

        public RepositoryDataTemplate() : base(CreateRepositoryDataTemplate)
        {
        }

        enum Row { TopPadding, Title, Description, DescriptionPadding, Statistics, BottomPadding }
        enum Column { Avatar, AvatarPadding, Emoji1, Statistic1, Emoji2, Statistic2, Emoji3, Statistic3, RightPadding }

        static Grid CreateRepositoryDataTemplate() => new Grid
        {
            BackgroundColor = Color.Transparent,

            Padding = new Thickness(2, 0, 5, 0),
            RowSpacing = 2,
            ColumnSpacing = 3,

            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.StartAndExpand,

            RowDefinitions = Rows.Define(
                (Row.TopPadding, new GridLength(1, GridUnitType.Absolute)),
                (Row.Title, new GridLength(20, GridUnitType.Absolute)),
                (Row.Description, new GridLength(45, GridUnitType.Absolute)),
                (Row.DescriptionPadding, new GridLength(1, GridUnitType.Absolute)),
                (Row.Statistics, new GridLength(_smallFontSize + 2, GridUnitType.Absolute)),
                (Row.BottomPadding, new GridLength(5, GridUnitType.Absolute))),


            ColumnDefinitions = Columns.Define(
                (Column.Avatar, new GridLength(circleImageHeight, GridUnitType.Absolute)),
                (Column.AvatarPadding, new GridLength(2, GridUnitType.Absolute)),
                (Column.Emoji1, new GridLength(emojiColumnSize, GridUnitType.Absolute)),
                (Column.Statistic1, new GridLength(statisticColumnSize, GridUnitType.Absolute)),
                (Column.Emoji2, new GridLength(emojiColumnSize, GridUnitType.Absolute)),
                (Column.Statistic2, new GridLength(statisticColumnSize, GridUnitType.Absolute)),
                (Column.Emoji3, new GridLength(emojiColumnSize, GridUnitType.Absolute)),
                (Column.Statistic3, new GridLength(statisticColumnSize, GridUnitType.Absolute)),
                (Column.RightPadding, new GridLength(1, GridUnitType.Star))),

            Children =
            {
                new AvatarImage().Row(Row.TopPadding).Column(Column.Avatar).RowSpan(6)
                    .Bind(Image.SourceProperty, nameof(Repository.OwnerAvatarUrl)),
                new RepositoryNameLabel().Row(Row.Title).Column(Column.Emoji1).ColumnSpan(7)
                    .Bind(Label.TextProperty, nameof(Repository.Name)),
                new RepositoryDescriptionLabel().Row(Row.Description).Column(Column.Emoji1).ColumnSpan(7)
                    .Bind(Label.TextProperty, nameof(Repository.Description)),
                new SmallNavyBlueSVGImage("star.svg").Row(Row.Statistics).Column(Column.Emoji1),

                new DarkBlueLabel(_smallFontSize - 1).Row(Row.Statistics).Column(Column.Statistic1)
                    .Bind(Label.TextProperty, nameof(Repository.StarCount)),
                new SmallNavyBlueSVGImage("repo_forked.svg").Row(Row.Statistics).Column(Column.Emoji2),

                new DarkBlueLabel(_smallFontSize - 1).Row(Row.Statistics).Column(Column.Statistic2)
                    .Bind(Label.TextProperty, nameof(Repository.ForkCount)),
                new SmallNavyBlueSVGImage("issue_opened.svg").Row(Row.Statistics).Column(Column.Emoji3),

                new DarkBlueLabel(_smallFontSize - 1).Row(Row.Statistics).Column(Column.Statistic3)
                    .Bind(Label.TextProperty, nameof(Repository.IssuesCount))
            }
        };

        class AvatarImage : CircleImage
        {
            public AvatarImage()
            {
                HeightRequest = circleImageHeight;
                WidthRequest = circleImageHeight;
                HorizontalOptions = LayoutOptions.Start;
                VerticalOptions = LayoutOptions.Center;
            }
        }

        class SmallNavyBlueSVGImage : SvgImage
        {
            public SmallNavyBlueSVGImage(in string svgFileName)
                : base(svgFileName, () => (Color)Application.Current.Resources[nameof(BaseTheme.TextColor)])
            {
                HeightRequest = _smallFontSize;
            }
        }

        class RepositoryNameLabel : DarkBlueLabel
        {
            public RepositoryNameLabel()
            {
                FontAttributes = FontAttributes.Bold;
                HorizontalTextAlignment = TextAlignment.Start;
                VerticalTextAlignment = TextAlignment.Start;
                LineBreakMode = LineBreakMode.TailTruncation;
                HorizontalOptions = LayoutOptions.FillAndExpand;
            }
        }

        class RepositoryDescriptionLabel : DarkBlueLabel
        {
            public RepositoryDescriptionLabel()
            {
                FontSize = _smallFontSize;
                LineBreakMode = LineBreakMode.WordWrap;
                VerticalTextAlignment = TextAlignment.Start;
                FontAttributes = FontAttributes.Italic;
            }
        }

        class DarkBlueLabel : Label
        {
            public DarkBlueLabel(in double fontSize) : this() => FontSize = fontSize;

            public DarkBlueLabel()
            {
                HorizontalTextAlignment = TextAlignment.Start;
                VerticalTextAlignment = TextAlignment.End;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
            }
        }
    }
}
