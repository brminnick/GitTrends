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
        const int _emojiColumnSize = 32;
        const int _statsColumnSize = 30;
        const int _circleImageHeight = 62;
        const double _statsFontSize = 12;

        readonly SortingService _sortingService;

        public RepositoryDataTemplateSelector(SortingService sortingService) => _sortingService = sortingService;

        enum Row { Title, Description, DescriptionPadding, Separator, SeparatorPadding, Statistics }
        enum Column { Avatar, AvatarPadding, Trending, Emoji1, Statistic1, Emoji2, Statistic2, Emoji3, Statistic3 }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var repository = (Repository)item;

            return SortingConstants.GetSortingCategory(_sortingService.CurrentOption) switch
            {
                SortingCategory.Clones => new ClonesDataTemplate(repository),
                SortingCategory.Views => new ViewsDataTemplate(repository),
                SortingCategory.IssuesForks => new IssuesForksDataTemplate(repository),
                _ => throw new NotSupportedException()
            };
        }

        static bool ShouldDisplayValue<T>(IReadOnlyList<T> list) where T : BaseDailyModel => list.Any();

        class ClonesDataTemplate : RepositoryDataTemplate
        {
            public ClonesDataTemplate(Repository repository) : base(repository.OwnerAvatarUrl, repository.Name, repository.Description, repository.IsTrending, CreateViews(repository))
            {

            }

            static IEnumerable<View> CreateViews(Repository repository)
            {
                var shouldDisplayValues = ShouldDisplayValue(repository.DailyClonesList);

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
                var shouldDisplayValues = ShouldDisplayValue(repository.DailyViewsList);

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
                var shouldDisplayValues = ShouldDisplayValue(repository.DailyViewsList);

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
            public CardView(IEnumerable<View> children)
            {
                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

                RowDefinitions = Rows.Define(
                    (Row.TopPadding, AbsoluteGridLength(8)),
                    (Row.Card, StarGridLength(1)),
                    (Row.BottomPadding, AbsoluteGridLength(1)));

                ColumnDefinitions = Columns.Define(
                    (Column.LeftPadding, AbsoluteGridLength(1)),
                    (Column.Card, StarGridLength(1)),
                    (Column.RightPadding, AbsoluteGridLength(1)));

                Children.Add(new CardViewFrame(children).Row(Row.Card).Column(Column.Card));
            }

            enum Row { TopPadding, Card, BottomPadding }
            enum Column { LeftPadding, Card, RightPadding }

            class CardViewFrame : PancakeView
            {
                public CardViewFrame(IEnumerable<View> children)
                {
                    Padding = new Thickness(16, 16, 12, 8);
                    BorderThickness = 1;
                    CornerRadius = 4;
                    HasShadow = false;
                    Visual = VisualMarker.Material;
                    Content = new ContentGrid(children);

                    SetDynamicResource(BorderColorProperty, nameof(BaseTheme.CardBorderColor));
                    SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
                }

                class ContentGrid : Grid
                {
                    public ContentGrid(IEnumerable<View> children)
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand;
                        VerticalOptions = LayoutOptions.FillAndExpand;
                        ColumnSpacing = 2;

                        RowDefinitions = Rows.Define(
                            (RepositoryDataTemplateSelector.Row.Title, AbsoluteGridLength(25)),
                            (RepositoryDataTemplateSelector.Row.Description, AbsoluteGridLength(40)),
                            (RepositoryDataTemplateSelector.Row.DescriptionPadding, AbsoluteGridLength(4)),
                            (RepositoryDataTemplateSelector.Row.Separator, AbsoluteGridLength(1)),
                            (RepositoryDataTemplateSelector.Row.SeparatorPadding, AbsoluteGridLength(4)),
                            (RepositoryDataTemplateSelector.Row.Statistics, AbsoluteGridLength(_statsFontSize + 2)));

                        ColumnDefinitions = Columns.Define(
                            (RepositoryDataTemplateSelector.Column.Avatar, AbsoluteGridLength(_circleImageHeight)),
                            (RepositoryDataTemplateSelector.Column.AvatarPadding, AbsoluteGridLength(16)),
                            (RepositoryDataTemplateSelector.Column.Trending, StarGridLength(1)),
                            (RepositoryDataTemplateSelector.Column.Emoji1, AbsoluteGridLength(_emojiColumnSize)),
                            (RepositoryDataTemplateSelector.Column.Statistic1, AbsoluteGridLength(_statsColumnSize)),
                            (RepositoryDataTemplateSelector.Column.Emoji2, AbsoluteGridLength(_emojiColumnSize)),
                            (RepositoryDataTemplateSelector.Column.Statistic2, AbsoluteGridLength(_statsColumnSize)),
                            (RepositoryDataTemplateSelector.Column.Emoji3, AbsoluteGridLength(_emojiColumnSize)),
                            (RepositoryDataTemplateSelector.Column.Statistic3, AbsoluteGridLength(_statsColumnSize)));

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

        abstract class TrendingImage : RepositoryStatSVGImage
        {
            protected const double SvgWidthRequest = 62;
            protected const double SvgHeightRequest = 16;

            public TrendingImage() : base("trending_tag.svg", nameof(BaseTheme.CardTrendingStatsColor), SvgWidthRequest, SvgHeightRequest)
            {
                VerticalOptions = LayoutOptions.End;
            }
        }


        class RepositoryStatSVGImage : SvgImage
        {
            public RepositoryStatSVGImage(in string svgFileName, string baseThemeColor, double width = 0, double height = 0)
                : base(svgFileName, () => (Color)Application.Current.Resources[baseThemeColor])
            {
                Margin = new Thickness(0, 0, 3, 0);

                WidthRequest = (width != 0.00) ? width : _emojiColumnSize / 2;
                HeightRequest = (height != 0.00) ? height : _emojiColumnSize / 2;

                VerticalOptions = LayoutOptions.CenterAndExpand;
                HorizontalOptions = LayoutOptions.EndAndExpand;
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

        class StatisticsLabel : Label
        {
            public StatisticsLabel(in long number, in string textColorThemeName)
            {
                Text = GetNumberAsText(number);
                FontSize = _statsFontSize;
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

                    new RepositoryNameLabel(repositoryName).Row(Row.Title).Column(Column.Trending).ColumnSpan(9),

                    new RepositoryDescriptionLabel(repositoryDescription).Row(Row.Description).Column(Column.Trending).ColumnSpan(9),

                    new Separator().Row(Row.Separator).Column(Column.Trending).ColumnSpan(9),

                    //On smaller screens, display TrendingImage under the Avatar
                    isTrending
                        ? smallScreenTrendingImage.Row(Row.Statistics).Column(Column.Avatar)
                        : new SvgCachedImage(),

                    //On larger screens, display TrendingImage under the Separator
                    isTrending
                        ? largeScreenTrendingImage.Row(Row.Statistics).Column(Column.Trending)
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

                    SetDynamicResource(FontFamilyProperty, nameof(BaseTheme.RobotoBold));
                }
            }

            class RepositoryDescriptionLabel : PrimaryColorLabel
            {
                public RepositoryDescriptionLabel(in string text) : base(14, text)
                {
                    MaxLines = 2;
                    SetDynamicResource(FontFamilyProperty, nameof(BaseTheme.RobotoRegular));
                }
            }

            class Separator : BoxView
            {
                public Separator() => SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
            }
        }
    }
}