using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Sharpnado.MaterialFrame;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.MarkupExtensions;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    abstract class BaseRepositoryDataTemplate : DataTemplate
    {
        public const int TopPadding = 12;
        public const int BottomPadding = 4;

        const int _statsColumnSize = 40;
        const double _statisticsRowHeight = StatisticsLabel.StatisticsFontSize + 4;
        const double _emojiColumnSize = _statisticsRowHeight;
        readonly static bool _isSmallScreen = ScreenWidth <= 360;
        readonly static double _circleImageHeight = _isSmallScreen ? 52 : 62;

        protected BaseRepositoryDataTemplate(IEnumerable<View> parentDataTemplateChildren, Repository repository) : base(() => new CardView(parentDataTemplateChildren, repository))
        {

        }

        protected enum Row { Title, Description, DescriptionPadding, Separator, SeparatorPadding, Statistics }
        protected enum Column { Avatar, AvatarPadding, Trending, Emoji1, Statistic1, Emoji2, Statistic2, Emoji3, Statistic3 }

        class CardView : Grid
        {
            public CardView(in IEnumerable<View> parentDataTemplateChildren, in Repository repository)
            {
                RowSpacing = 0;
                RowDefinitions = Rows.Define(
                    (CardViewRow.TopPadding, AbsoluteGridLength(TopPadding)),
                    (CardViewRow.Card, Star),
                    (CardViewRow.BottomPadding, AbsoluteGridLength(BottomPadding)));

                ColumnDefinitions = Columns.Define(
                    (CardViewColumn.LeftPadding, AbsoluteGridLength(16)),
                    (CardViewColumn.Card, Star),
                    (CardViewColumn.RightPadding, AbsoluteGridLength(16)));

                Children.Add(new CardViewFrame(parentDataTemplateChildren, repository).Row(CardViewRow.Card).Column(CardViewColumn.Card));

                this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            }

            enum CardViewRow { TopPadding, Card, BottomPadding }
            enum CardViewColumn { LeftPadding, Card, RightPadding }

            class CardViewFrame : MaterialFrame
            {
                public CardViewFrame(in IEnumerable<View> parentDataTemplateChildren, in Repository repository)
                {
                    Padding = new Thickness(16, 16, 12, 8);
                    CornerRadius = 4;
                    HasShadow = false;
                    Elevation = 4;

                    Content = new ContentGrid(parentDataTemplateChildren, repository);

                    this.DynamicResource(MaterialThemeProperty, nameof(BaseTheme.MaterialFrameTheme));
                }

                class ContentGrid : Grid
                {
                    public ContentGrid(in IEnumerable<View> parentDataTemplateChildren, in Repository repository)
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
                            (Column.AvatarPadding, AbsoluteGridLength(_isSmallScreen ? 8 : 16)),
                            (Column.Trending, StarGridLength(1)),
                            (Column.Emoji1, AbsoluteGridLength(_emojiColumnSize)),
                            (Column.Statistic1, AbsoluteGridLength(_statsColumnSize)),
                            (Column.Emoji2, AbsoluteGridLength(_emojiColumnSize)),
                            (Column.Statistic2, AbsoluteGridLength(_statsColumnSize)),
                            (Column.Emoji3, AbsoluteGridLength(_emojiColumnSize)),
                            (Column.Statistic3, AbsoluteGridLength(_statsColumnSize)));

                        Children.Add(new AvatarImage(repository.OwnerAvatarUrl)
                                        .Row(Row.Title).Column(Column.Avatar).RowSpan(2)
                                        .Bind(CircleImage.ImageSourceProperty, nameof(Repository.OwnerAvatarUrl)));

                        Children.Add(new NameLabel(repository.Name)
                                        .Row(Row.Title).Column(Column.Trending).ColumnSpan(7));

                        Children.Add(new DescriptionLabel(repository.Description)
                                        .Row(Row.Description).Column(Column.Trending).ColumnSpan(7)
                                        .Bind(Label.TextProperty, nameof(Repository.Description)));

                        Children.Add(new Separator()
                                        .Row(Row.Separator).Column(Column.Trending).ColumnSpan(7));

                        //On large screens, display TrendingImage in the same column as the repository name
                        Children.Add(new LargeScreenTrendingImage().Assign(out LargeScreenTrendingImage largeScreenTrendingImage)
                                        .Row(Row.SeparatorPadding).Column(Column.Trending).RowSpan(2));

                        //On smaller screens, display TrendingImage under the Avatar
                        Children.Add(new SmallScreenTrendingImage(largeScreenTrendingImage)
                                        .Row(Row.SeparatorPadding).Column(Column.Avatar).RowSpan(2).ColumnSpan(3));

                        foreach (var child in parentDataTemplateChildren)
                        {
                            Children.Add(child);
                        }
                    }

                    class AvatarImage : CircleImage
                    {
                        public AvatarImage(in string avatarUrl)
                        {
                            ImageSource = avatarUrl;
                            WidthRequest = _circleImageHeight;

                            Border = new Border { Thickness = 1 };

                            this.Center();
                            this.DynamicResources((BorderColorProperty, nameof(BaseTheme.SeparatorColor)),
                                                    (ErrorPlaceholderProperty, nameof(BaseTheme.DefaultProfileImageSource)),
                                                    (LoadingPlaceholderProperty, nameof(BaseTheme.DefaultProfileImageSource)));
                        }
                    }

                    class NameLabel : PrimaryColorLabel
                    {
                        public NameLabel(in string name) : base(20)
                        {
                            Text = name;

                            LineBreakMode = LineBreakMode.TailTruncation;
                            HorizontalOptions = LayoutOptions.FillAndExpand;
                            FontFamily = FontFamilyConstants.RobotoBold;
                        }
                    }

                    class DescriptionLabel : PrimaryColorLabel
                    {
                        public DescriptionLabel(in string description) : base(14)
                        {
                            Text = description;

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

                            this.DynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
                        }
                    }

                    class Separator : BoxView
                    {
                        public Separator() => this.DynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
                    }

                    class LargeScreenTrendingImage : TrendingImage
                    {
                        public LargeScreenTrendingImage() : base(RepositoryPageAutomationIds.LargeScreenTrendingImage)
                        {
                            SetBinding(IsVisibleProperty, new MultiBinding
                            {
                                Converter = new IsVisibleConverter(largeScreenTrendingImageWidth => largeScreenTrendingImageWidth >= SvgWidthRequest),
                                Bindings =
                                {
                                    new Binding(nameof(Repository.IsTrending)),
                                    new Binding(nameof(Width), source: this)
                                }
                            });
                        }
                    }

                    class SmallScreenTrendingImage : TrendingImage
                    {
                        public SmallScreenTrendingImage(LargeScreenTrendingImage largeScreenTrendingImage) : base(RepositoryPageAutomationIds.SmallScreenTrendingImage)
                        {
                            SetBinding(IsVisibleProperty, new MultiBinding
                            {
                                Converter = new IsVisibleConverter(largeScreenTrendingImageWidth => largeScreenTrendingImageWidth < SvgWidthRequest),
                                Bindings =
                                {
                                    new Binding(nameof(Repository.IsTrending)),
                                    new Binding(nameof(Width), source: largeScreenTrendingImage)
                                }
                            });
                        }
                    }

                    abstract class TrendingImage : StatisticsSvgImage
                    {
                        public const double SvgWidthRequest = 62;
                        public const double SvgHeightRequest = 16;

                        public TrendingImage(string automationId) : base("trending_tag.svg", nameof(BaseTheme.CardTrendingStatsColor), SvgWidthRequest, SvgHeightRequest)
                        {
                            AutomationId = automationId;
                            HorizontalOptions = LayoutOptions.Start;
                            VerticalOptions = LayoutOptions.End;
                        }

                        protected class IsVisibleConverter : IMultiValueConverter
                        {
                            readonly Func<double, bool> _isWidthValid;

                            public IsVisibleConverter(Func<double, bool> isWidthValid) => _isWidthValid = isWidthValid;

                            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
                            {
                                if (values is null || !values.Any())
                                    return false;

                                if (values[0] is bool isTrending && isTrending is true
                                    && values[1] is double width)
                                {
                                    // When `Width is -1`, Xamarin.Forms hasn't inflated the View
                                    // Allow Xamarin.Forms to inflate the view, then validate its Width
                                    if (width is -1 || _isWidthValid(width))
                                        return true;

                                    return false;
                                }

                                return false;
                            }

                            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
                        }
                    }
                }
            }
        }
    }
}
