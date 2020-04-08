using System;
using System.Collections.Generic;
using System.Linq;
using FFImageLoading.Svg.Forms;
using GitTrends.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class RepositoryDataTemplateSelector : DataTemplateSelector
    {
        const int _statsColumnSize = 40;
        const int _circleImageHeight = 62;
        const double _statsFontSize = 12;
        const double _statisticsRowHeight = _statsFontSize + 4;
        const double _emojiColumnSize = _statisticsRowHeight;

        readonly SortingService _sortingService;

        public RepositoryDataTemplateSelector(SortingService sortingService) => _sortingService = sortingService;

        enum Row { Title, Description, DescriptionPadding, Separator, SeparatorPadding, Statistics }
        enum Column { Avatar, AvatarPadding, Trending, Emoji1, Statistic1, Emoji2, Statistic2, Emoji3, Statistic3 }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var repository = (Repository)item;
            var sortingCategory = SortingConstants.GetSortingCategory(_sortingService.CurrentOption);

            return sortingCategory switch
            {
                SortingCategory.Clones => new ClonesDataTemplate(repository),
                SortingCategory.Views => new ViewsDataTemplate(repository),
                SortingCategory.IssuesForks => new IssuesForksDataTemplate(repository),
                _ => throw new NotSupportedException()
            };
        }

        class ClonesDataTemplate : RepositoryDataTemplate
        {
            public ClonesDataTemplate(Repository repository) : base(repository.OwnerAvatarUrl, repository.Name, repository.Description, repository.IsTrending, CreateViews(repository))
            {

            }

            static IEnumerable<View> CreateViews(Repository repository)
            {
                var shouldDisplayValues = repository.DailyClonesList.Any();

                return new View[]
                {
                    new RepositoryStatSVGImage("total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValues
                        ? new StatisticsLabel(repository.TotalClones, nameof(BaseTheme.CardClonesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1)
                        : new Label(),

                    new RepositoryStatSVGImage("unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValues
                        ? new StatisticsLabel(repository.TotalUniqueClones, nameof(BaseTheme.CardUniqueClonesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2)
                        : new Label(),

                    new RepositoryStatSVGImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValues
                        ? new StatisticsLabel(repository.StarCount, nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3)
                        : new Label()
                };
            }
        }

        class ViewsDataTemplate : RepositoryDataTemplate
        {
            public ViewsDataTemplate(Repository repository) : base(repository.OwnerAvatarUrl, repository.Name, repository.Description, repository.IsTrending, CreateViews(repository))
            {

            }

            static IEnumerable<View> CreateViews(Repository repository)
            {
                var shouldDisplayValues = repository.DailyViewsList.Any();

                return new View[]
                {
                    new RepositoryStatSVGImage("total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValues
                        ? new StatisticsLabel(repository.TotalViews, nameof(BaseTheme.CardViewsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1)
                        : new Label(),

                    new RepositoryStatSVGImage("unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValues
                        ? new StatisticsLabel(repository.TotalUniqueViews, nameof(BaseTheme.CardUniqueViewsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2)
                        : new Label(),

                    new RepositoryStatSVGImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValues
                        ? new StatisticsLabel(repository.StarCount, nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3)
                        : new Label()
                };
            }
        }

        class IssuesForksDataTemplate : RepositoryDataTemplate
        {
            public IssuesForksDataTemplate(Repository repository) : base(repository.OwnerAvatarUrl, repository.Name, repository.Description, repository.IsTrending, CreateViews(repository))
            {

            }

            static IEnumerable<View> CreateViews(Repository repository)
            {
                var shouldDisplayValues = repository.DailyViewsList.Any();

                return new View[]
                {
                    new RepositoryStatSVGImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValues
                        ? new StatisticsLabel(repository.StarCount, nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1)
                        : new Label(),

                    new RepositoryStatSVGImage("repo_forked.svg", nameof(BaseTheme.CardForksStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValues
                        ? new StatisticsLabel(repository.ForkCount, nameof(BaseTheme.CardForksStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2)
                        : new Label(),

                    new RepositoryStatSVGImage("issue_opened.svg", nameof(BaseTheme.CardIssuesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValues
                        ? new StatisticsLabel(repository.IssuesCount, nameof(BaseTheme.CardIssuesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3)
                        : new Label()
                };
            }
        }

        class CardView : Grid
        {
            public CardView(in IEnumerable<View> children)
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

                Children.Add(new CardViewFrame(children).Row(CardViewRow.Card).Column(CardViewColumn.Card));

                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));
            }

            enum CardViewRow { TopPadding, Card, BottomPadding }
            enum CardViewColumn { LeftPadding, Card, RightPadding }

            class CardViewFrame : PancakeView
            {
                public CardViewFrame(in IEnumerable<View> children)
                {
                    Padding = new Thickness(16, 16, 12, 8);
                    CornerRadius = 4;
                    HasShadow = false;
                    BorderThickness = 2;
                    Content = new ContentGrid(children);

                    SetDynamicResource(BorderColorProperty, nameof(BaseTheme.CardBorderColor));
                    SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
                }

                class ContentGrid : Grid
                {
                    public ContentGrid(in IEnumerable<View> children)
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand;
                        VerticalOptions = LayoutOptions.FillAndExpand;

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

                        foreach (var child in children)
                        {
                            Children.Add(child);
                        }
                    }
                }
            }
        }

        class LargeScreenTrendingImage : TrendingImage
        {
            public LargeScreenTrendingImage()
            {
                SetBinding(IsVisibleProperty, new Binding(nameof(Width), converter: IsVisibleWidthConverter, source: this));
            }

            static FuncConverter<double, bool> IsVisibleWidthConverter { get; } = new FuncConverter<double, bool>(width => width is -1 || width >= SvgWidthRequest);
        }


        class SmallScreenTrendingImage : TrendingImage
        {
            public SmallScreenTrendingImage(in LargeScreenTrendingImage largeScreenTrendingImage)
            {
                SetBinding(IsVisibleProperty, new Binding(nameof(IsVisible), converter: IsLargeScreenTrendingImageNotVisible, source: largeScreenTrendingImage));
            }

            static FuncConverter<bool, bool> IsLargeScreenTrendingImageNotVisible { get; } = new FuncConverter<bool, bool>(isLargeScreenTrendingImageVisible => !isLargeScreenTrendingImageVisible);
        }

        class RepositoryStatSVGImage : SvgImage
        {
            public RepositoryStatSVGImage(in string svgFileName, string baseThemeColor, in double widthRequest = 24, in double heightRequest = 24)
                : base(svgFileName, () => (Color)Application.Current.Resources[baseThemeColor], widthRequest, heightRequest)
            {
                VerticalOptions = LayoutOptions.CenterAndExpand;
                HorizontalOptions = LayoutOptions.EndAndExpand;
            }
        }
        class StatisticsLabel : Label
        {
            public StatisticsLabel(in long number, in string textColorThemeName)
            {
                Text = GetNumberAsText(number);
                FontSize = _statsFontSize;

                HorizontalOptions = LayoutOptions.FillAndExpand;

                HorizontalTextAlignment = TextAlignment.Start;
                VerticalTextAlignment = TextAlignment.End;

                LineBreakMode = LineBreakMode.TailTruncation;

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

        abstract class TrendingImage : RepositoryStatSVGImage
        {
            protected const double SvgWidthRequest = 62;
            protected const double SvgHeightRequest = 16;

            public TrendingImage() : base("trending_tag.svg", nameof(BaseTheme.CardTrendingStatsColor), SvgWidthRequest, SvgHeightRequest)
            {
                HorizontalOptions = LayoutOptions.Start;
                VerticalOptions = LayoutOptions.End;
            }
        }

        abstract class PrimaryColorLabel : Label
        {
            protected PrimaryColorLabel(in double fontSize, in string text)
            {
                FontSize = fontSize;
                Text = text;
                LineBreakMode = LineBreakMode.TailTruncation;
                HorizontalTextAlignment = TextAlignment.Start;
                VerticalTextAlignment = TextAlignment.Start;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
            }
        }

        abstract class RepositoryDataTemplate : DataTemplate
        {
            protected RepositoryDataTemplate(string ownerAvatarUrl, string repositoryName, string repositoryDescription, bool isTrending, IEnumerable<View> views)
                : base(() => CreateRepositoryDataTemplate(ownerAvatarUrl, repositoryName, repositoryDescription, isTrending, views))
            {

            }

            static CardView CreateRepositoryDataTemplate(in string ownerAvatarUrl, in string repositoryName, in string repositoryDescription, in bool isTrending, in IEnumerable<View> views)
            {
                var largeScreenTrendingImage = new LargeScreenTrendingImage();
                var smallScreenTrendingImage = new SmallScreenTrendingImage(largeScreenTrendingImage);

                var repositoryDataTemplateViews = new View[]
                {
                    new AvatarImage(ownerAvatarUrl).Row(Row.Title).Column(Column.Avatar).RowSpan(2).Center(),

                    new RepositoryNameLabel(repositoryName).Row(Row.Title).Column(Column.Trending).ColumnSpan(7),

                    new RepositoryDescriptionLabel(repositoryDescription).Row(Row.Description).Column(Column.Trending).ColumnSpan(7),

                    new Separator().Row(Row.Separator).Column(Column.Trending).ColumnSpan(7),

                    //On smaller screens, display TrendingImage under the Avatar
                    isTrending
                        ? smallScreenTrendingImage.Row(Row.SeparatorPadding).Column(Column.Avatar).RowSpan(2).ColumnSpan(3)
                        : new SvgCachedImage(),

                    //On larger screens, display TrendingImage under the Separator
                    isTrending
                        ? largeScreenTrendingImage.Row(Row.SeparatorPadding).Column(Column.Trending).RowSpan(2)
                        : new SvgCachedImage(),
                };

                return new CardView(views.Concat(repositoryDataTemplateViews));
            }

            class AvatarImage : CircleImage
            {
                public AvatarImage(string imageSource)
                {
                    HeightRequest = _circleImageHeight;
                    WidthRequest = _circleImageHeight;
                    HorizontalOptions = LayoutOptions.Center;
                    VerticalOptions = LayoutOptions.Center;
                    BorderThickness = 1;
                    Source = imageSource;

                    SetDynamicResource(BorderColorProperty, nameof(BaseTheme.SeparatorColor));
                }
            }

            class RepositoryNameLabel : PrimaryColorLabel
            {
                public RepositoryNameLabel(in string text) : base(20, text)
                {
                    LineBreakMode = LineBreakMode.TailTruncation;
                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    FontFamily = FontFamilyConstants.RobotoBold;
                }
            }

            class RepositoryDescriptionLabel : PrimaryColorLabel
            {
                public RepositoryDescriptionLabel(in string text) : base(14, text)
                {
                    MaxLines = 2;
                    LineHeight = 1.16;
                    FontFamily = FontFamilyConstants.RobotoRegular;
                }
            }

            class Separator : BoxView
            {
                public Separator() => SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
            }
        }
    }
}