using System;
using System.Collections.Generic;
using GitTrends.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    abstract class BaseRepositoryDataTemplate : DataTemplate
    {
        const int _statsColumnSize = 40;
        const int _circleImageHeight = 62;
        const double _statisticsRowHeight = StatisticsLabel.StatiscsFontSize + 4;
        const double _emojiColumnSize = _statisticsRowHeight;

        protected BaseRepositoryDataTemplate(IEnumerable<View> parentDataTemplateChildren) : base(() => new CardView(parentDataTemplateChildren))
        {

        }

        protected enum Row { Title, Description, DescriptionPadding, Separator, SeparatorPadding, Statistics }
        protected enum Column { Avatar, AvatarPadding, Trending, Emoji1, Statistic1, Emoji2, Statistic2, Emoji3, Statistic3 }

        class CardView : Grid
        {
            public CardView(in IEnumerable<View> parentDataTemplateChildren)
            {
                RowSpacing = 0;
                RowDefinitions = Rows.Define(
                    (CardViewRow.TopPadding, AbsoluteGridLength(8)),
                    (CardViewRow.Card, StarGridLength(1)),
                    (CardViewRow.BottomPadding, AbsoluteGridLength(8)));

                ColumnDefinitions = Columns.Define(
                    (CardViewColumn.LeftPadding, AbsoluteGridLength(16)),
                    (CardViewColumn.Card, StarGridLength(1)),
                    (CardViewColumn.RightPadding, AbsoluteGridLength(16)));

                Children.Add(new CardViewFrame(parentDataTemplateChildren).Row(CardViewRow.Card).Column(CardViewColumn.Card));

                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            }

            enum CardViewRow { TopPadding, Card, BottomPadding }
            enum CardViewColumn { LeftPadding, Card, RightPadding }

            class CardViewFrame : PancakeView
            {
                public CardViewFrame(in IEnumerable<View> parentDataTemplateChildren)
                {
                    Padding = new Thickness(16, 16, 12, 8);
                    CornerRadius = 4;
                    HasShadow = false;
                    BorderThickness = 2;
                    Content = new ContentGrid(parentDataTemplateChildren);

                    SetDynamicResource(BorderColorProperty, nameof(BaseTheme.CardBorderColor));
                    SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
                }

                class ContentGrid : Grid
                {
                    public ContentGrid(in IEnumerable<View> parentDataTemplateChildren)
                    {
                        this.FillExpand();

                        RowDefinitions = Rows.Define(
                            (Row.Title, AbsoluteGridLength(25)),
                            (Row.Description, AbsoluteGridLength(40)),
                            (Row.DescriptionPadding, AbsoluteGridLength(4)),
                            (Row.Separator, AbsoluteGridLength(1)),
                            (Row.SeparatorPadding, AbsoluteGridLength(4)),
                            (Row.Statistics, AbsoluteGridLength(_statisticsRowHeight)));

                        ColumnDefinitions = Columns.Define(
                            (Column.Avatar, AbsoluteGridLength(_circleImageHeight)),
                            (Column.AvatarPadding, AbsoluteGridLength(16)),
                            (Column.Trending, StarGridLength(1)),
                            (Column.Emoji1, AbsoluteGridLength(_emojiColumnSize)),
                            (Column.Statistic1, AbsoluteGridLength(_statsColumnSize)),
                            (Column.Emoji2, AbsoluteGridLength(_emojiColumnSize)),
                            (Column.Statistic2, AbsoluteGridLength(_statsColumnSize)),
                            (Column.Emoji3, AbsoluteGridLength(_emojiColumnSize)),
                            (Column.Statistic3, AbsoluteGridLength(_statsColumnSize)));

                        var largeScreenTrendingImage = new LargeScreenTrendingImage();

                        Children.Add(new AvatarImage()
                                        .Row(Row.Title).Column(Column.Avatar).RowSpan(2)
                                        .Bind(Image.SourceProperty, nameof(Repository.OwnerAvatarUrl)));

                        Children.Add(new NameLabel()
                                        .Row(Row.Title).Column(Column.Trending).ColumnSpan(7)
                                        .Bind(Label.TextProperty, nameof(Repository.Name)));

                        Children.Add(new DescriptionLabel()
                                        .Row(Row.Description).Column(Column.Trending).ColumnSpan(7)
                                        .Bind(Label.TextProperty, nameof(Repository.Description)));

                        Children.Add(new Separator()
                                        .Row(Row.Separator).Column(Column.Trending).ColumnSpan(7));

                        //On smaller screens, display TrendingImage under the Avatar
                        Children.Add(new TrendingImage()
                                        .Row(Row.SeparatorPadding).Column(Column.Avatar).RowSpan(2).ColumnSpan(3)
                                        .Bind(IsVisibleProperty, nameof(Repository.IsTrending))
                                        .Bind<TrendingImage, double, Func<Color>>(SvgImage.GetTextColorProperty, nameof(Width), source: largeScreenTrendingImage, convert: largeScreenTrendingImageTextColorConverter));

                        //On large screens, display TrendingImage in the same column as the repository name
                        Children.Add(largeScreenTrendingImage
                                        .Row(Row.SeparatorPadding).Column(Column.Trending).RowSpan(2)
                                        .Bind(IsVisibleProperty, nameof(Repository.IsTrending)));

                        foreach (var child in parentDataTemplateChildren)
                        {
                            Children.Add(child);
                        }

                        //Reveal the tag if the LargeScreenTrendingImage is not shown by changing its color from matching the CardSurfaceColor
                        static Func<Color> largeScreenTrendingImageTextColorConverter(double largeTrendingImageWidth)
                        {
                            if (largeTrendingImageWidth >= 0 && largeTrendingImageWidth < TrendingImage.SvgWidthRequest)
                                return () => (Color)Application.Current.Resources[nameof(BaseTheme.CardTrendingStatsColor)];
                            else
                                return () => (Color)Application.Current.Resources[nameof(BaseTheme.CardSurfaceColor)];
                        }
                    }

                    class AvatarImage : CircleImage
                    {
                        public AvatarImage()
                        {
                            this.Center();

                            HeightRequest = _circleImageHeight;
                            WidthRequest = _circleImageHeight;

                            BorderThickness = 1;

                            SetDynamicResource(BorderColorProperty, nameof(BaseTheme.SeparatorColor));
                        }
                    }

                    class NameLabel : PrimaryColorLabel
                    {
                        public NameLabel() : base(20)
                        {
                            LineBreakMode = LineBreakMode.TailTruncation;
                            HorizontalOptions = LayoutOptions.FillAndExpand;
                            FontFamily = FontFamilyConstants.RobotoBold;
                        }
                    }

                    class DescriptionLabel : PrimaryColorLabel
                    {
                        public DescriptionLabel() : base(14)
                        {
                            MaxLines = 2;
                            LineHeight = 1.16;
                            FontFamily = FontFamilyConstants.RobotoRegular;
                        }
                    }

                    abstract class PrimaryColorLabel : Label
                    {
                        protected PrimaryColorLabel(in double fontSize)
                        {
                            FontSize = fontSize;
                            LineBreakMode = LineBreakMode.TailTruncation;
                            HorizontalTextAlignment = TextAlignment.Start;
                            VerticalTextAlignment = TextAlignment.Start;

                            SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
                        }
                    }

                    class Separator : BoxView
                    {
                        public Separator() => SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
                    }

                    class LargeScreenTrendingImage : TrendingImage
                    {
                        protected override void OnSizeAllocated(double width, double height)
                        {
                            base.OnSizeAllocated(width, height);

                            //Reveal the tag `if (width >= SvgWidthRequest)` by changing its color to CardTrendingStatsColor from the default color which matches the CardSurfaceColor
                            if (!IsVisible)
                                return;
                            else if (width >= SvgWidthRequest)
                                GetTextColor = () => (Color)Application.Current.Resources[nameof(BaseTheme.CardTrendingStatsColor)];
                            else
                                GetTextColor = () => (Color)Application.Current.Resources[nameof(BaseTheme.CardSurfaceColor)];
                        }
                    }

                    class TrendingImage : StatisticsSvgImage
                    {
                        public const double SvgWidthRequest = 62;
                        public const double SvgHeightRequest = 16;

                        //Set default color to match the Card Surface Color to "hide" the tag
                        public TrendingImage() : base("trending_tag.svg", nameof(BaseTheme.CardSurfaceColor), SvgWidthRequest, SvgHeightRequest)
                        {
                            HorizontalOptions = LayoutOptions.Start;
                            VerticalOptions = LayoutOptions.End;
                        }
                    }
                }
            }
        }
    }
}
