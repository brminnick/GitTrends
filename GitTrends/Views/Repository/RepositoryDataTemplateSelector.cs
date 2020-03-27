using System;
using System.Collections.Generic;
using System.Linq;
using FFImageLoading.Svg.Forms;
using GitTrends.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class RepositoryDataTemplateSelector : DataTemplateSelector
    {
        const int circleImageHeight = 62;
        const int emojiColumnSize = 24;
        const int statisticColumnSize = 32;

        const int _statsFontSize = 11;

        readonly SortingService _sortingService;

        public RepositoryDataTemplateSelector(SortingService sortingService) => _sortingService = sortingService;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => new RepositoryDataTemplate((Repository)item, ShouldShowStarkForksIssues(_sortingService));

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

            enum Row { Title, Description, DescriptionPadding, Separator, SeparatorPadding, Statistics}
            enum Column { Avatar, AvatarPadding, Trending, Emoji1, Statistic1, Emoji2, Statistic2, Emoji3, Statistic3, Emoji4, Statistic4, StatisticsPadding }

            static CardView CreateRepositoryDataTemplate(Repository repository, bool shouldShowStarsForksIssues) => new CardView
            {

                Content = new Grid
                {
                    BackgroundColor = Color.Transparent,

                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    ColumnSpacing = 0,

                    RowDefinitions = Rows.Define(
                    (Row.Title, Auto),
                    (Row.Description, Auto),
                    (Row.DescriptionPadding, AbsoluteGridLength(12)),
                    (Row.Separator, AbsoluteGridLength(1)),
                    (Row.SeparatorPadding, AbsoluteGridLength(4)),
                    (Row.Statistics, AbsoluteGridLength(_statsFontSize + 2))),

                    ColumnDefinitions = Columns.Define(
                    (Column.Avatar, AbsoluteGridLength(circleImageHeight)),
                    (Column.AvatarPadding, AbsoluteGridLength(16)),
                    (Column.Trending, StarGridLength(1)),
                    (Column.Emoji1, Auto),
                    (Column.Statistic1, Auto),
                    (Column.Emoji2, Auto),
                    (Column.Statistic2, Auto),
                    (Column.Emoji3, Auto),
                    (Column.Statistic3, Auto),
                    (Column.Emoji4, Auto),
                    (Column.Statistic4, Auto),
                    (Column.StatisticsPadding, AbsoluteGridLength(4))),


                    Children =
                    {
                        new AvatarImage().Row(Row.Title).Column(Column.Avatar).RowSpan(2).Center()
                            .Bind(Image.SourceProperty, nameof(Repository.OwnerAvatarUrl)),

                        new RepositoryNameLabel(repository.Name).Row(Row.Title).Column(Column.Trending).ColumnSpan(10),

                        new RepositoryDescriptionLabel(repository.Description).Row(Row.Description).Column(Column.Trending).ColumnSpan(10),

                        new Separator().Row(Row.Separator).Column(Column.Trending).ColumnSpan(10),

                        //Only display the Trending label when a repository is trending
                        repository.IsTrending
                            ? new RepoStatSVGImage("trending_tag.svg", () => (Color)Application.Current.Resources[nameof(BaseTheme.CardTrendingStatsColor)], 62, 16).Row(Row.SeparatorPadding).Column(Column.Trending).RowSpan(2)
                        .Margin(new Thickness(8,0,0,0)).Start().Bottom()
                            : new SvgCachedImage(),

                        new RepoStatSVGImage(shouldShowStarsForksIssues ? "star.svg" : "total_views.svg", () => (Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsIconColor)])
                        .Row(Row.Statistics).Column(Column.Emoji1).Margin(new Thickness(0,0,4,0)).End(),

                        //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                        shouldDisplayValue(repository.DailyViewsList)
                            ? new CustomColorLabel(_statsFontSize, (Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsTextColor)], shouldShowStarsForksIssues ? repository.StarCount.ToString() : repository.TotalViews.ToString())
                        .Row(Row.Statistics).Column(Column.Statistic1).End()
                            : new Label(), 

                        new RepoStatSVGImage(shouldShowStarsForksIssues ? "repo_forked.svg" : "unique_views.svg", () => (Color)Application.Current.Resources[nameof(BaseTheme.CardForksStatsIconColor)])
                        .Row(Row.Statistics).Column(Column.Emoji2).Margin(new Thickness(16,0,4,0)).End().End(),

                        //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                        shouldDisplayValue(repository.DailyViewsList)
                            ? new CustomColorLabel(_statsFontSize, (Color)Application.Current.Resources[nameof(BaseTheme.CardForksStatsTextColor)], shouldShowStarsForksIssues ? repository.ForkCount.ToString() : repository.TotalUniqueViews.ToString()).Row(Row.Statistics).Column(Column.Statistic2).End()
                            : new Label(),

                        new RepoStatSVGImage(shouldShowStarsForksIssues ? "issue_opened.svg" : "total_clones.svg", () => (Color)Application.Current.Resources[nameof(BaseTheme.CardIssuesStatsIconColor)])
                        .Row(Row.Statistics).Column(Column.Emoji3).Margin(new Thickness(16,0,4,0)).End(),

                        //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                        shouldDisplayValue(repository.DailyClonesList)
                            ? new CustomColorLabel(_statsFontSize, (Color)Application.Current.Resources[nameof(BaseTheme.CardIssuesStatsTextColor)], shouldShowStarsForksIssues ? repository.IssuesCount.ToString() : repository.TotalClones.ToString()).Row(Row.Statistics).Column(Column.Statistic3).End()
                            : new Label(),

                        //Column.Emoji4 & Column.Statistic4 are not needed for StarsForksIssues
                        !shouldShowStarsForksIssues
                            ? new RepoStatSVGImage("unique_clones.svg", () => (Color)Application.Current.Resources[nameof(BaseTheme.CardUniqueClonesStatsIconColor)])
                        .Row(Row.Statistics).Column(Column.Emoji4).Margin(new Thickness(16,0,4,0)).End()
                            : new SvgCachedImage(),

                        //Column.Emoji4 & Column.Statistic4 are not needed for StarsForksIssues
                        //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                        !shouldShowStarsForksIssues && shouldDisplayValue(repository.DailyClonesList)
                            ? new CustomColorLabel(_statsFontSize, (Color)Application.Current.Resources[nameof(BaseTheme.CardUniqueClonesStatsTextColor)],repository.TotalUniqueClones.ToString()).Row(Row.Statistics).Column(Column.Statistic4).End()
                            : new Label(),

                        
                    }
                }
            };

            static bool shouldDisplayValue<T>(IList<T> list) where T : BaseDailyModel => list.Any();
        }

        class CardView : Frame
        {
            public CardView()
            {
                CornerRadius = 4;
                HasShadow = false;
                Visual = VisualMarker.Material;
                Padding = new Thickness(16, 16, 12, 8);

                SetDynamicResource(BorderColorProperty, nameof(BaseTheme.CardBorderColor));
                SetDynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardSurfaceColor));
            }
        }

        class AvatarImage : CircleImage
        {
            public AvatarImage()
            {
                HeightRequest = circleImageHeight;
                WidthRequest = circleImageHeight;
                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Center;
                BorderThickness = 1;
                SetDynamicResource(BorderColorProperty, nameof(BaseTheme.SeparatorColor));
            }
        }

        class RepoStatSVGImage : SvgImage
        {
            public RepoStatSVGImage(in string svgFileName, in Func<Color> func, double width = 0.00, double height = 0.00)
                : base(svgFileName, func)
            {
                
                WidthRequest = (width != 0.00) ? width : _statsFontSize;
                HeightRequest = (height != 0.00) ? height : _statsFontSize;

                VerticalOptions = LayoutOptions.CenterAndExpand;
                HorizontalOptions = LayoutOptions.CenterAndExpand;
            }
        }

        class RepositoryNameLabel : PrimaryColorLabel
        {
            public RepositoryNameLabel(in string text) : base(text)
            {
                FontSize = 20;
                HorizontalTextAlignment = TextAlignment.Start;
                VerticalTextAlignment = TextAlignment.Start;
                LineBreakMode = LineBreakMode.TailTruncation;
                HorizontalOptions = LayoutOptions.FillAndExpand;
                FontFamily = "Roboto-Bold";
            }
        }

        class RepositoryDescriptionLabel : PrimaryColorLabel
        {
            public RepositoryDescriptionLabel(in string text) : base(text)
            {
                FontSize = 14;
                LineBreakMode = LineBreakMode.TailTruncation;
                VerticalTextAlignment = TextAlignment.Start;
                MaxLines = 2;
                FontFamily = "Roboto-Regular";
            }
        }

        class PrimaryColorLabel : Label
        {
            public PrimaryColorLabel(in double fontSize, in string text) : this(text) => FontSize = fontSize;

            public PrimaryColorLabel(in string text)
            {
                Text = text;

                HorizontalTextAlignment = TextAlignment.Start;
                VerticalTextAlignment = TextAlignment.End;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
            }
        }


        class CustomColorLabel : Label
        {
            public CustomColorLabel(in double fontSize, Color textColor, in string text) : this(text)
            {
                FontSize = fontSize;
                TextColor = textColor;
            }

            public CustomColorLabel(in string text)
            {
                Text = text;

                HorizontalTextAlignment = TextAlignment.Start;
                VerticalTextAlignment = TextAlignment.End;
            }
        }


        class Separator : BoxView
        {
            public Separator()
            {
                SetDynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
            }
        }
    }
}
