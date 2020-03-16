using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FFImageLoading.Svg.Forms;
using GitTrends.Shared;
using ImageCircle.Forms.Plugin.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class RepositoryDataTemplateSelector : DataTemplateSelector
    {
        const int circleImageHeight = 90;
        const int emojiColumnSize = 15;
        const int statisticColumnSize = 30;

        const int _smallFontSize = 12;

        readonly SortingService _sortingService;

        public RepositoryDataTemplateSelector(SortingService sortingService) => _sortingService = sortingService;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => new RepositoryDataTemplate((Repository)item, ShouldShowStarkForksIssues(_sortingService));

        static bool ShouldShowStarkForksIssues(SortingService sortingService) => sortingService.CurrentOption switch
        {
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

            enum Row { TopPadding, Title, Description, DescriptionPadding, Statistics, BottomPadding }
            enum Column { Avatar, AvatarPadding, Emoji1, Statistic1, Emoji2, Statistic2, Emoji3, Statistic3, Emoji4, Statistic4, RightPadding }

            static Grid CreateRepositoryDataTemplate(Repository repository, bool shouldShowStarsForksIssues) => new Grid
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
                    (Column.Emoji4, new GridLength(emojiColumnSize, GridUnitType.Absolute)),
                    (Column.Statistic4, new GridLength(statisticColumnSize, GridUnitType.Absolute)),
                    (Column.RightPadding, new GridLength(1, GridUnitType.Star))),

                Children =
                {
                    new AvatarImage().Row(Row.TopPadding).Column(Column.Avatar).RowSpan(6)
                        .Bind(Image.SourceProperty, nameof(Repository.OwnerAvatarUrl)),

                    new RepositoryNameLabel(repository.Name).Row(Row.Title).Column(Column.Emoji1).ColumnSpan(9),

                    new RepositoryDescriptionLabel(repository.Description).Row(Row.Description).Column(Column.Emoji1).ColumnSpan(9),

                    new SmallNavyBlueSVGImage(shouldShowStarsForksIssues ? "star.svg" : "total_views.svg").Row(Row.Statistics).Column(Column.Emoji1),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValue(repository.DailyClonesList)
                        ? new DarkBlueLabel(_smallFontSize - 1, shouldShowStarsForksIssues ? repository.StarCount.ToString() : repository.TotalViews.ToString()).Row(Row.Statistics).Column(Column.Statistic1)
                        : new Label(),

                    new SmallNavyBlueSVGImage(shouldShowStarsForksIssues ? "repo_forked.svg" : "unique_views.svg").Row(Row.Statistics).Column(Column.Emoji2),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValue(repository.DailyClonesList)
                        ? new DarkBlueLabel(_smallFontSize - 1, shouldShowStarsForksIssues ? repository.ForkCount.ToString() : repository.TotalUniqueViews.ToString()).Row(Row.Statistics).Column(Column.Statistic2)
                        : new Label(),

                    new SmallNavyBlueSVGImage(shouldShowStarsForksIssues ? "issue_opened.svg" : "total_clones.svg").Row(Row.Statistics).Column(Column.Emoji3),

                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    shouldDisplayValue(repository.DailyClonesList)
                        ? new DarkBlueLabel(_smallFontSize - 1, shouldShowStarsForksIssues ? repository.IssuesCount.ToString() : repository.TotalClones.ToString()).Row(Row.Statistics).Column(Column.Statistic3)
                        : new Label(),

                    //Column.Emoji4 & Column.Statistic4 are not needed for StarsForksIssues
                    !shouldShowStarsForksIssues
                        ? new SmallNavyBlueSVGImage("unique_clones.svg").Row(Row.Statistics).Column(Column.Emoji4)
                        : new SvgCachedImage(),

                    //Column.Emoji4 & Column.Statistic4 are not needed for StarsForksIssues
                    //Only display the value when the Repository Data finishes loading. This avoid showing '0' while the data is loading.
                    !shouldShowStarsForksIssues && shouldDisplayValue(repository.DailyClonesList)
                        ? new DarkBlueLabel(_smallFontSize - 1, repository.TotalUniqueClones.ToString()).Row(Row.Statistics).Column(Column.Statistic4)
                        : new Label()
                }
            };

            static bool shouldDisplayValue<T>(IList<T> list) where T : BaseDailyModel => list.Any();
        }

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
            public RepositoryNameLabel(in string text) : base(text)
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
            public RepositoryDescriptionLabel(in string text) : base(text)
            {
                FontSize = _smallFontSize;
                LineBreakMode = LineBreakMode.WordWrap;
                VerticalTextAlignment = TextAlignment.Start;
                FontAttributes = FontAttributes.Italic;
            }
        }

        class DarkBlueLabel : Label
        {
            public DarkBlueLabel(in double fontSize, in string text) : this(text) => FontSize = fontSize;

            public DarkBlueLabel(in string text)
            {
                Text = text;

                HorizontalTextAlignment = TextAlignment.Start;
                VerticalTextAlignment = TextAlignment.End;

                SetDynamicResource(TextColorProperty, nameof(BaseTheme.TextColor));
            }
        }
    }
}
