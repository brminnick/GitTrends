using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        const int _emojiColumnSize = 20;
        const int _statsColumnSize = 30;
        const int _circleImageHeight = 62;
        const double _statsFontSize = 8.5;

        readonly SortingService _sortingService;

        public RepositoryDataTemplateSelector(SortingService sortingService) => _sortingService = sortingService;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
            new RepositoryDataTemplate((Repository)item, ShouldShowStarkForksIssues(_sortingService));

        static bool ShouldShowStarkForksIssues(SortingService sortingService) => sortingService.CurrentOption switch
        {
            SortingOption.Trending => false,
            SortingOption.Clones => false,
            SortingOption.UniqueClones => false,
            SortingOption.Views => false,
            SortingOption.UniqueViews => false,
            SortingOption.Forks => true,
            SortingOption.Issues => true,
            SortingOption.Stars => true,
            _ => throw new NotSupportedException()
        };

        class RepositoryDataTemplate : DataTemplate
        {
            public RepositoryDataTemplate(Repository repository, bool shouldShowStarsForksIssues) : base(() => CreateRepositoryDataTemplate(repository, shouldShowStarsForksIssues))
            {

            }

            enum Row { Title, Description, DescriptionPadding, Separator, SeparatorPadding, Statistics }
            enum Column { Avatar, AvatarPadding, Trending, Emoji1, Statistic1, Emoji2, Statistic2, Emoji3, Statistic3, Emoji4, Statistic4 }

            static CardView CreateRepositoryDataTemplate(Repository repository, bool shouldShowStarsForksIssues)
            {
                return new CardView(new Grid
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    ColumnSpacing = 2,

                    RowDefinitions = Rows.Define(
                        (Row.Title, AbsoluteGridLength(25)),
                        (Row.Description, AbsoluteGridLength(40)),
                        (Row.DescriptionPadding, AbsoluteGridLength(4)),
                        (Row.Separator, AbsoluteGridLength(1)),
                        (Row.SeparatorPadding, AbsoluteGridLength(4)),
                        (Row.Statistics, AbsoluteGridLength(_statsFontSize + 2))),

                    ColumnDefinitions = Columns.Define(
                        (Column.Avatar, AbsoluteGridLength(_circleImageHeight)),
                        (Column.AvatarPadding, AbsoluteGridLength(16)),
                        (Column.Trending, StarGridLength(1)),
                        (Column.Emoji1, AbsoluteGridLength(_emojiColumnSize)),
                        (Column.Statistic1, AbsoluteGridLength(_statsColumnSize)),
                        (Column.Emoji2, AbsoluteGridLength(_emojiColumnSize)),
                        (Column.Statistic2, AbsoluteGridLength(_statsColumnSize)),
                        (Column.Emoji3, AbsoluteGridLength(_emojiColumnSize)),
                        (Column.Statistic3, AbsoluteGridLength(_statsColumnSize)),
                        (Column.Emoji4, AbsoluteGridLength(_emojiColumnSize)),
                        (Column.Statistic4, AbsoluteGridLength(_statsColumnSize))),

                    Children =
                    {
                        new AvatarImage(repository.OwnerAvatarUrl).Row(Row.Title).Column(Column.Avatar).RowSpan(2).Center(),

                        new RepositoryNameLabel(repository.Name).Row(Row.Title).Column(Column.Trending).ColumnSpan(9),

                        new RepositoryDescriptionLabel(repository.Description).Row(Row.Description).Column(Column.Trending).ColumnSpan(9),

                        new Separator().Row(Row.Separator).Column(Column.Trending).ColumnSpan(9),

                        //Only display the Trending label when a repository is trending
                        repository.IsTrending
                            ? new RepositoryStatSVGImage("trending_tag.svg", nameof(BaseTheme.CardTrendingStatsColor), 62, 16).Row(Row.SeparatorPadding).Column(Column.Avatar).ColumnSpan(3).RowSpan(2).EndExpand().Bottom()
                            : new SvgCachedImage(),

                        new RepositoryStatSVGImage(shouldShowStarsForksIssues ? "star.svg" : "total_views.svg", nameof(BaseTheme.CardStarsStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji1),

                        //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                        shouldDisplayValue(repository.DailyViewsList)
                            ? new StatisticsLabel(shouldShowStarsForksIssues ? repository.StarCount : repository.TotalViews, nameof(BaseTheme.CardStarsStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic1)
                            : new Label(),

                        new RepositoryStatSVGImage(shouldShowStarsForksIssues ? "repo_forked.svg" : "unique_views.svg", nameof(BaseTheme.CardForksStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji2),

                        //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                        shouldDisplayValue(repository.DailyViewsList)
                            ? new StatisticsLabel(shouldShowStarsForksIssues ? repository.ForkCount : repository.TotalUniqueViews, nameof(BaseTheme.CardForksStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic2)
                            : new Label(),

                        new RepositoryStatSVGImage(shouldShowStarsForksIssues ? "issue_opened.svg" : "total_clones.svg", nameof(BaseTheme.CardIssuesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji3),

                        //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                        shouldDisplayValue(repository.DailyClonesList)
                            ? new StatisticsLabel(shouldShowStarsForksIssues ? repository.IssuesCount : repository.TotalClones, nameof(BaseTheme.CardIssuesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic3)
                            : new Label(),

                        //Column.Emoji4 & Column.Statistic4 are not needed for StarsForksIssues
                        !shouldShowStarsForksIssues
                            ? new RepositoryStatSVGImage("unique_clones.svg", nameof(BaseTheme.CardUniqueClonesStatsIconColor)).Row(Row.Statistics).Column(Column.Emoji4)
                            : new SvgCachedImage(),

                        //Column.Emoji4 & Column.Statistic4 are not needed for StarsForksIssues
                        //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                        !shouldShowStarsForksIssues && shouldDisplayValue(repository.DailyClonesList)
                            ? new StatisticsLabel(repository.TotalUniqueClones, nameof(BaseTheme.CardUniqueClonesStatsTextColor)).Row(Row.Statistics).Column(Column.Statistic4)
                            : new Label()
                    }
                });

                static bool shouldDisplayValue<T>(IReadOnlyList<T> list) where T : BaseDailyModel => list.Any();
            }

            class CardView : ContentView
            {
                public CardView(View content)
                {
                    Content = new Grid
                    {
                        RowDefinitions = Rows.Define(
                            (Row.TopPadding, AbsoluteGridLength(8)),
                            (Row.Card, StarGridLength(1))),

                        ColumnDefinitions = Columns.Define(
                            (Column.LeftPadding, AbsoluteGridLength(1)),
                            (Column.Card, StarGridLength(1)),
                            (Column.RightPadding, AbsoluteGridLength(1))),

                        Children =
                        {
                            new CardViewFrame(content).Row(Row.Card).Column(Column.Card)
                        }
                    };
                }

                enum Row { TopPadding, Card }
                enum Column { LeftPadding, Card, RightPadding }

                class CardViewFrame : PancakeView
                {
                    public CardViewFrame(View view)
                    {
                        Padding = new Thickness(16, 16, 12, 8);
                        BorderThickness = 1;
                        CornerRadius = 4;
                        HasShadow = false;
                        Visual = VisualMarker.Material;
                        Content = view;

                        SetDynamicResource(BorderColorProperty, nameof(BaseTheme.CardBorderColor));
                        SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
                    }
                }
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

            class Separator : BoxView
            {
                public Separator() => SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
            }
        }
    }
}
