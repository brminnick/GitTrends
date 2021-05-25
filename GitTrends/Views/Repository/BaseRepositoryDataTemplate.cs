using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Sharpnado.MaterialFrame;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using static GitTrends.XamarinFormsService;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    abstract class BaseRepositoryDataTemplate : DataTemplate
    {
        public const int TopPadding = 12;
        public const int BottomPadding = 4;

        const int _statsColumnSize = 40;
        const double _statisticsRowHeight = StatisticsLabel.StatisticsFontSize + 4;
        const double _emojiColumnSize = _statisticsRowHeight;
        readonly static double _circleImageHeight = IsSmallScreen ? 52 : 62;

        readonly static AsyncAwaitBestPractices.WeakEventManager _tappedWeakEventManager = new();

        protected BaseRepositoryDataTemplate(IEnumerable<View> parentDataTemplateChildren, RepositoryViewModel repositoryViewModel, Repository repository) : base(() => new CardView(parentDataTemplateChildren, repositoryViewModel, repository))
        {

        }

        public static event EventHandler Tapped
        {
            add => _tappedWeakEventManager.AddEventHandler(value);
            remove => _tappedWeakEventManager.RemoveEventHandler(value);
        }

        protected enum Row { Title, Description, DescriptionPadding, Separator, SeparatorPadding, Statistics }
        protected enum Column { Avatar, AvatarPadding, Trending, Emoji1, Statistic1, Emoji2, Statistic2, Emoji3, Statistic3 }

        class CardView : ExtendedSwipeView<Repository>
        {
            public CardView(in IEnumerable<View> dataTemplateChildren, in RepositoryViewModel repositoryViewModel, in Repository repository)
            {
                Tapped += HandleTapped;

                BackgroundColor = Color.Transparent;

                RightItems = new SwipeItems
                {
                    new SwipeItemView
                    {
                        CommandParameter = repository,
                        Command = repositoryViewModel.ToggleIsFavoriteCommand,
                        Content = new SvgImage(repository.IsFavorite is true ? "star.svg" : "star_outline.svg", () => (Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsIconColor)], 44, 44)
                                    .Margin(new Thickness(0, 0, 32, 0))
                    }
                };

                Content = new Grid
                {
                    RowSpacing = 0,

                    RowDefinitions = Rows.Define(
                        (CardViewRow.TopPadding, TopPadding),
                        (CardViewRow.Card, Star),
                        (CardViewRow.BottomPadding, BottomPadding)),

                    ColumnDefinitions = Columns.Define(
                        (CardViewColumn.LeftPadding, IsSmallScreen ? 8 : 16),
                        (CardViewColumn.Card, Star),
                        (CardViewColumn.RightPadding, IsSmallScreen ? 8 : 16)),

                    Children =
                    {
                        new CardViewFrame(dataTemplateChildren, repository).Row(CardViewRow.Card).Column(CardViewColumn.Card)
                    }
                };
            }

            enum CardViewRow { TopPadding, Card, BottomPadding }
            enum CardViewColumn { LeftPadding, Card, RightPadding }

            void HandleTapped(object sender, EventArgs e) => _tappedWeakEventManager.RaiseEvent(this, e, nameof(BaseRepositoryDataTemplate.Tapped));

            class CardViewFrame : MaterialFrame
            {
                public CardViewFrame(in IEnumerable<View> dataTemplateChildren, in Repository repository)
                {
                    Padding = IsSmallScreen ? new Thickness(8, 16, 6, 8) : new Thickness(16, 16, 12, 8);
                    CornerRadius = 4;
                    HasShadow = false;
                    Elevation = 4;

                    Content = new ContentGrid(dataTemplateChildren, repository);

                    this.DynamicResource(MaterialThemeProperty, nameof(BaseTheme.MaterialFrameTheme));
                }

                class ContentGrid : Grid
                {
                    public ContentGrid(in IEnumerable<View> dataTemplateChildren, in Repository repository)
                    {
                        this.FillExpand();

                        RowDefinitions = Rows.Define(
                            (Row.Title, 25),
                            (Row.Description, 40),
                            (Row.DescriptionPadding, 4),
                            (Row.Separator, 1),
                            (Row.SeparatorPadding, 4),
                            (Row.Statistics, _statisticsRowHeight));

                        ColumnDefinitions = Columns.Define(
                            (Column.Avatar, IsSmallScreen ? _circleImageHeight - 4 : _circleImageHeight),
                            (Column.AvatarPadding, IsSmallScreen ? 4 : 16),
                            (Column.Trending, Star),
                            (Column.Emoji1, _emojiColumnSize),
                            (Column.Statistic1, _statsColumnSize),
                            (Column.Emoji2, _emojiColumnSize),
                            (Column.Statistic2, _statsColumnSize),
                            (Column.Emoji3, _emojiColumnSize),
                            (Column.Statistic3, _statsColumnSize));

                        Children.Add(new AvatarImage(repository.OwnerAvatarUrl, _circleImageHeight)
                                        .Row(Row.Title).Column(Column.Avatar).RowSpan(2)
                                        .DynamicResources((CircleImage.BorderColorProperty, nameof(BaseTheme.SeparatorColor)),
                                                            (CircleImage.ErrorPlaceholderProperty, nameof(BaseTheme.DefaultProfileImageSource)),
                                                            (CircleImage.LoadingPlaceholderProperty, nameof(BaseTheme.DefaultProfileImageSource))));

                        Children.Add(new NameLabel(repository.Name)
                                        .Row(Row.Title).Column(Column.Trending).ColumnSpan(7));

                        Children.Add(new DescriptionLabel(repository.Description)
                                        .Row(Row.Description).Column(Column.Trending).ColumnSpan(7));

                        Children.Add(new Separator()
                                        .Row(Row.Separator).Column(Column.Trending).ColumnSpan(7));

                        //On large screens, display TrendingImage in the same column as the repository name
                        Children.Add(new LargeScreenTrendingImage(repository.IsTrending, repository.IsFavorite).Assign(out LargeScreenTrendingImage largeScreenTrendingImage)
                                        .Row(Row.SeparatorPadding).Column(Column.Trending).RowSpan(2));

                        //On smaller screens, display TrendingImage under the Avatar
                        Children.Add(new SmallScreenTrendingImage(repository.IsTrending, repository.IsFavorite, largeScreenTrendingImage)
                                        .Row(Row.SeparatorPadding).Column(Column.Avatar).RowSpan(2).ColumnSpan(3));

                        foreach (var child in dataTemplateChildren)
                        {
                            Children.Add(child);
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
                        public LargeScreenTrendingImage(bool isTrending, bool? isFavorite) : base(RepositoryPageAutomationIds.LargeScreenTrendingImage, isTrending, isFavorite)
                        {
                            SetBinding(IsVisibleProperty, new MultiBinding
                            {
                                Converter = new IsVisibleConverter(largeScreenTrendingImageWidth => largeScreenTrendingImageWidth >= SvgWidthRequest),
                                Bindings =
                                {
                                    new Binding(nameof(Repository.IsTrending), BindingMode.OneWay),
                                    new Binding(nameof(Width), BindingMode.OneWay, source: this),
                                    new Binding(nameof(Repository.IsFavorite), BindingMode.OneWay)
                                }
                            });
                        }
                    }

                    class SmallScreenTrendingImage : TrendingImage
                    {
                        public SmallScreenTrendingImage(bool isTrending, bool? isFavorite, LargeScreenTrendingImage largeScreenTrendingImage) :
                            base(RepositoryPageAutomationIds.SmallScreenTrendingImage, isTrending, isFavorite)
                        {
                            SetBinding(IsVisibleProperty, new MultiBinding
                            {
                                Converter = new IsVisibleConverter(largeScreenTrendingImageWidth => largeScreenTrendingImageWidth < SvgWidthRequest),
                                Bindings =
                                {
                                    new Binding(nameof(Repository.IsTrending), BindingMode.OneWay),
                                    new Binding(nameof(Width), BindingMode.OneWay, source: largeScreenTrendingImage),
                                    new Binding(nameof(Repository.IsFavorite), BindingMode.OneWay)
                                }
                            });
                        }
                    }

                    abstract class TrendingImage : StatisticsSvgImage
                    {
                        public const double SvgWidthRequest = 62;
                        public const double SvgHeightRequest = 16;

                        public TrendingImage(string automationId, bool isTrending, bool? isFavorite)
                            : base(isTrending ? "trending_tag.svg" : "favorite_tag.svg",
                                    isFavorite is true ? nameof(BaseTheme.CardStarsStatsIconColor) : nameof(BaseTheme.CardTrendingStatsColor),
                                    SvgWidthRequest,
                                    SvgHeightRequest)
                        {
                            AutomationId = automationId;
                            HorizontalOptions = LayoutOptions.Start;
                            VerticalOptions = LayoutOptions.End;
                        }

                        protected class IsVisibleConverter : IMultiValueConverter
                        {
                            readonly Func<double, bool> _isWidthValid;

                            public IsVisibleConverter(Func<double, bool> isWidthValid) => _isWidthValid = isWidthValid;

                            public object? Convert(object?[]? values, Type? targetType, object? parameter, CultureInfo? culture)
                            {
                                if (values is null || !values.Any())
                                    return false;

                                if (values[0] is null || values[1] is null)
                                    return false;

                                var isTrending = (bool)(values[0] ?? throw new NotSupportedException("Value cannot be null"));
                                var width = (double)(values[1] ?? throw new NotSupportedException("Value cannot be null"));
                                var isFavorite = (bool?)values[2];

                                // When `Width is -1`, Xamarin.Forms hasn't inflated the View
                                // Allow Xamarin.Forms to inflate the view, then validate its Width
                                return (isTrending || isFavorite is true)
                                        && (width is -1 || _isWidthValid(width));
                            }

                            public object?[]? ConvertBack(object? value, Type?[]? targetTypes, object? parameter, CultureInfo? culture) => throw new NotImplementedException();
                        }
                    }
                }
            }
        }
    }
}
