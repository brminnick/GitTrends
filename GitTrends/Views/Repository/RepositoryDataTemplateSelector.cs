using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GitTrends.Mobile.Shared;
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
            var sortingCategory = SortingConstants.GetSortingCategory(_sortingService.CurrentOption);

            return sortingCategory switch
            {
                SortingCategory.Clones => new ClonesDataTemplate(),
                SortingCategory.Views => new ViewsDataTemplate(),
                SortingCategory.IssuesForks => new IssuesForksDataTemplate(),
                _ => throw new NotSupportedException()
            };
        }

        class ClonesDataTemplate : RepositoryDataTemplate
        {
            public ClonesDataTemplate() : base(CreateClonesDataTemplateViews())
            {

            }

            static IEnumerable<View> CreateClonesDataTemplateViews() => new View[]
            {
                new RepositoryStatSVGImage("total_clones.svg", nameof(BaseTheme.CardClonesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardClonesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.TotalClones), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyClonesModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyClonesList), convert: IsStatisticsLabelVisibleConverter),

                new RepositoryStatSVGImage("unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardUniqueClonesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.TotalUniqueClones), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyClonesModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyClonesList), convert: IsStatisticsLabelVisibleConverter),

                new RepositoryStatSVGImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.StarCount), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyClonesModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyClonesList), convert: IsStatisticsLabelVisibleConverter),
            };
        }

        class ViewsDataTemplate : RepositoryDataTemplate
        {
            public ViewsDataTemplate() : base(CreateViewsDataTemplateViews())
            {

            }

            static IEnumerable<View> CreateViewsDataTemplateViews() => new View[]
            {
                new RepositoryStatSVGImage("total_views.svg", nameof(BaseTheme.CardViewsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardViewsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.TotalViews), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),

                new RepositoryStatSVGImage("unique_views.svg", nameof(BaseTheme.CardUniqueViewsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardUniqueViewsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.TotalUniqueViews), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),

                new RepositoryStatSVGImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.StarCount), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),
            };
        }

        class IssuesForksDataTemplate : RepositoryDataTemplate
        {
            public IssuesForksDataTemplate() : base(CreateIssuesForksDataTemplateViews())
            {

            }

            static IEnumerable<View> CreateIssuesForksDataTemplateViews() => new View[]
            {
                new RepositoryStatSVGImage("star.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.StarCount), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),

                new RepositoryStatSVGImage("repo_forked.svg", nameof(BaseTheme.CardForksStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardForksStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.ForkCount), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),

                new RepositoryStatSVGImage("issue_opened.svg", nameof(BaseTheme.CardIssuesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                new StatisticsLabel(nameof(BaseTheme.CardIssuesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3)
                    .Bind<StatisticsLabel, long, string>(Label.TextProperty, nameof(Repository.IssuesCount), convert: StatisticsLabelTextConverter)
                    .Bind<StatisticsLabel, IReadOnlyList<DailyViewsModel>, bool>(VisualElement.IsVisibleProperty, nameof(Repository.DailyViewsList), convert: IsStatisticsLabelVisibleConverter),
            };
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

                        foreach (var child in children)
                        {
                            Children.Add(child);
                        }
                    }
                }
            }
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
            public StatisticsLabel(in string textColorThemeName)
            {
                FontSize = _statsFontSize;

                HorizontalOptions = LayoutOptions.FillAndExpand;

                HorizontalTextAlignment = TextAlignment.Start;
                VerticalTextAlignment = TextAlignment.End;

                LineBreakMode = LineBreakMode.TailTruncation;

                SetDynamicResource(TextColorProperty, textColorThemeName);
            }
        }

        class LargeScreenTrendingImage : TrendingImage
        {
            protected override void OnSizeAllocated(double width, double height)
            {
                base.OnSizeAllocated(width, height);

                //Reveal the tag `if (width >= SvgWidthRequest)` by changing its color from the default color which matches the CardSurfaceColor
                if (IsVisible && width >= SvgWidthRequest)
                    GetTextColor = () => (Color)Application.Current.Resources[nameof(BaseTheme.CardTrendingStatsColor)];
                else if (IsVisible)
                    GetTextColor = () => (Color)Application.Current.Resources[nameof(BaseTheme.CardSurfaceColor)];
            }
        }

        class TrendingImage : RepositoryStatSVGImage
        {
            protected const double SvgWidthRequest = 62;
            protected const double SvgHeightRequest = 16;

            //Set default color to match the Card Surface Color to "hide" the tag
            public TrendingImage() : base("trending_tag.svg", nameof(BaseTheme.CardSurfaceColor), SvgWidthRequest, SvgHeightRequest)
            {
                HorizontalOptions = LayoutOptions.Start;
                VerticalOptions = LayoutOptions.End;
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

        abstract class RepositoryDataTemplate : DataTemplate
        {
            protected RepositoryDataTemplate(IEnumerable<View> views)
                : base(() => CreateRepositoryDataTemplate(views))
            {

            }

            protected static bool IsStatisticsLabelVisibleConverter(IEnumerable<BaseDailyModel> dailyModels) => dailyModels.Any();
            protected static string StatisticsLabelTextConverter(long number) => number.ConvertToAbbreviatedText();

            static CardView CreateRepositoryDataTemplate(in IEnumerable<View> views)
            {
                var largeScreenTrendingImage = new LargeScreenTrendingImage();

                var repositoryDataTemplateViews = new View[]
                {
                    new AvatarImage().Row(Row.Title).Column(Column.Avatar).RowSpan(2)
                        .Bind(Image.SourceProperty, nameof(Repository.OwnerAvatarUrl)),

                    new RepositoryNameLabel().Row(Row.Title).Column(Column.Trending).ColumnSpan(7)
                        .Bind(Label.TextProperty,nameof(Repository.Name)),

                    new RepositoryDescriptionLabel().Row(Row.Description).Column(Column.Trending).ColumnSpan(7)
                        .Bind(Label.TextProperty,nameof(Repository.Description)),

                    new Separator().Row(Row.Separator).Column(Column.Trending).ColumnSpan(7),

                    //On smaller screens, display TrendingImage under the Avatar
                    new TrendingImage().Row(Row.SeparatorPadding).Column(Column.Avatar).RowSpan(2).ColumnSpan(3)
                        .Bind(VisualElement.IsVisibleProperty, nameof(Repository.IsTrending))
                        .Bind(SvgImage.GetTextColorProperty,nameof(SvgImage.GetTextColor), source: largeScreenTrendingImage, converter: LargeScreenTrendingImageTextColorConverter),

                    //On large screens, display TrendingImage in the same column as the repository name
                    largeScreenTrendingImage.Row(Row.SeparatorPadding).Column(Column.Trending).RowSpan(2)
                        .Bind(VisualElement.IsVisibleProperty, nameof(Repository.IsTrending)),
                };

                return new CardView(views.Concat(repositoryDataTemplateViews));
            }

            //Reveal the tag if the LargeScreenTrendingImage is not shown by changing its color from matching the CardSurfaceColor
            static FuncConverter<Func<Color>, Func<Color>> LargeScreenTrendingImageTextColorConverter { get; } = new FuncConverter<Func<Color>, Func<Color>>(getLargeScreenTextColor =>
            {
                if (getLargeScreenTextColor() == (Color)Application.Current.Resources[nameof(BaseTheme.CardSurfaceColor)])
                    return () => (Color)Application.Current.Resources[nameof(BaseTheme.CardTrendingStatsColor)];
                else
                    return () => (Color)Application.Current.Resources[nameof(BaseTheme.CardSurfaceColor)];
            });

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

            class RepositoryNameLabel : PrimaryColorLabel
            {
                public RepositoryNameLabel() : base(20)
                {
                    LineBreakMode = LineBreakMode.TailTruncation;
                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    FontFamily = FontFamilyConstants.RobotoBold;
                }
            }

            class RepositoryDescriptionLabel : PrimaryColorLabel
            {
                public RepositoryDescriptionLabel() : base(14)
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