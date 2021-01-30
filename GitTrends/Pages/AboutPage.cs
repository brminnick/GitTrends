using System;
using System.Linq;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
    class AboutPage : BaseContentPage<AboutViewModel>
    {
        readonly DeepLinkingService _deepLinkingService;

        public AboutPage(IMainThread mainThread,
                            AboutViewModel aboutViewModel,
                            IAnalyticsService analyticsService,
                            DeepLinkingService deepLinkingService) : base(aboutViewModel, analyticsService, mainThread)
        {
            const int titleColumnWidth = 68;
            const int horizontalPadding = 32;
            const int separatorColumnWidth = 1;

            _deepLinkingService = deepLinkingService;

            Title = AboutPageConstants.About;
            Padding = new Thickness(0, 16, 0, 0);

            Content = new Grid
            {
                ColumnSpacing = 2,

                ColumnDefinitions = Columns.Define(
                    (Column.LeftPadding, horizontalPadding),
                    (Column.Icon, Star),
                    (Column.Watching, titleColumnWidth),
                    (Column.WatchingSeparator, separatorColumnWidth),
                    (Column.Stars, titleColumnWidth),
                    (Column.StarsSeparator, separatorColumnWidth),
                    (Column.Forks, titleColumnWidth),
                    (Column.RightPadding, horizontalPadding)),

                RowDefinitions = Rows.Define(
                    (Row.Title, 30),
                    (Row.Description, 50),
                    (Row.StatsTitle, 12),
                    (Row.StatsNumber, 16),
                    (Row.ActionButtons, 35),
                    (Row.CollaboratorTitle, 30),
                    (Row.CollaboratorDescription, 40),
                    (Row.CollaboratorCollection, 100),
                    (Row.LibrariesTitle, 10),
                    (Row.LibrariesDescription, 15),
                    (Row.LibrariesCollection, Star)),

                Children =
                {
                     new Image().CenterExpand()
                        .Row(Row.Title).RowSpan(4).Column(Column.Icon)
                        .DynamicResource(Image.SourceProperty, nameof(BaseTheme.GitTrendsImageSource)),

                    new Label { Text = "GitTrends" }.Font(FontFamilyConstants.RobotoMedium, 24)
                        .Row(Row.Title).Column(Column.Watching).ColumnSpan(5)
                        .DynamicResource(Label.TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor)),

                    new DescriptionLabel("GitTrends is an open-source app to help monitor ") { MaxLines = 3 }
                        .Row(Row.Description).Column(Column.Watching).ColumnSpan(5),

                    new StatsTitleLayout("Watching", "unique_views.svg", () => (Color)Application.Current.Resources[nameof(BaseTheme.SettingsLabelTextColor)])
                        .Row(Row.StatsTitle).Column(Column.Watching),

                    new BoxView()
                        .Row(Row.StatsTitle).RowSpan(2).Column(Column.WatchingSeparator)
                        .DynamicResource(BackgroundColorProperty,nameof(BaseTheme.SettingsLabelTextColor)),

                    new StatsTitleLayout("Stars", "star.svg",() => (Color)Application.Current.Resources[nameof(BaseTheme.SettingsLabelTextColor)])
                        .Row(Row.StatsTitle).Column(Column.Stars),

                    new BoxView()
                        .Row(Row.StatsTitle).RowSpan(2).Column(Column.StarsSeparator)
                        .DynamicResource(BackgroundColorProperty,nameof(BaseTheme.SettingsLabelTextColor)),

                    new StatsTitleLayout("Forks", "repo_forked.svg",() => (Color)Application.Current.Resources[nameof(BaseTheme.SettingsLabelTextColor)])
                        .Row(Row.StatsTitle).Column(Column.Forks),

                    new Label { BackgroundColor = Color.CornflowerBlue, Text = "Watching Number" }
                        .Row(Row.StatsNumber).Column(Column.Watching),

                    new Label { BackgroundColor = Color.Cornsilk, Text = "Stars Number" }
                        .Row(Row.StatsNumber).Column(Column.Stars),

                    new Label { BackgroundColor = Color.Crimson, Text = "Forks Number" }
                        .Row(Row.StatsNumber).Column(Column.Forks),

                    new Label { BackgroundColor = Color.BlueViolet, Text = "View on GitHub" }
                        .Row(Row.ActionButtons).Column(Column.Icon).ColumnSpan(3),

                    new Label { BackgroundColor = Color.Brown, Text = "Request Feature" }
                        .Row(Row.ActionButtons).Column(Column.WatchingSeparator).ColumnSpan(4),

                    new TitleLabel("Collaborators")
                        .Row(Row.CollaboratorTitle).Column(Column.Icon).ColumnSpan(6),

                    new DescriptionLabel("Thank You to all of our amazing open-source contributors!")
                        .Row(Row.CollaboratorDescription).Column(Column.Icon).ColumnSpan(6),

                    new CollectionView
                    {
                        Header = new BoxView { WidthRequest = horizontalPadding },
                        SelectionMode = SelectionMode.Single,
                        ItemTemplate = new ContributorDataTemplate(),
                        ItemsSource = ViewModel.GitTrendsContributors,
                        ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
                    }.Center()
                     .Row(Row.CollaboratorCollection).ColumnSpan(All<Column>())
                     .Invoke(collectionView => collectionView.SelectionChanged += HandleContributorSelectionChanged),

                    new TitleLabel("Libraries")
                        .Row(Row.LibrariesTitle).Column(Column.Icon).ColumnSpan(6),

                    new DescriptionLabel("GitTrends leverages following libraries and frameworks")
                        .Row(Row.LibrariesDescription).Column(Column.Icon).ColumnSpan(6),

                    new Label { BackgroundColor = Color.Coral, Text = "Library Collection" }
                        .Row(Row.LibrariesCollection).ColumnSpan(All<Column>()),
                }
            };
        }

        enum Row { Title, Description, StatsTitle, StatsNumber, ActionButtons, CollaboratorTitle, CollaboratorDescription, CollaboratorCollection, LibrariesTitle, LibrariesDescription, LibrariesCollection }
        enum Column { LeftPadding, Icon, Watching, WatchingSeparator, Stars, StarsSeparator, Forks, RightPadding }

        async void HandleContributorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;
            collectionView.SelectedItem = null;

            if (e.CurrentSelection.FirstOrDefault() is Contributor contributor)
            {
                AnalyticsService.Track("Contributor Tapped", nameof(Contributor.Login), contributor.Login);

                await _deepLinkingService.OpenBrowser(contributor.GitHubUrl);
            }
        }

        class TitleLabel : Label
        {
            public TitleLabel(in string text)
            {
                Text = text;
                FontSize = 16;

                HorizontalOptions = LayoutOptions.StartAndExpand;
                VerticalOptions = LayoutOptions.StartAndExpand;

                FontFamily = FontFamilyConstants.RobotoMedium;
                LineBreakMode = LineBreakMode.TailTruncation;

                this.DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
            }
        }

        class DescriptionLabel : Label
        {
            public DescriptionLabel(in string text)
            {
                Text = text;
                FontSize = 12;

                HorizontalOptions = LayoutOptions.StartAndExpand;
                VerticalOptions = LayoutOptions.StartAndExpand;

                FontFamily = FontFamilyConstants.RobotoRegular;
                LineBreakMode = LineBreakMode.TailTruncation;

                this.DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
            }
        }

        class StatsTitleLayout : StackLayout
        {
            public StatsTitleLayout(in string text, in string svgFileName, in Func<Color> getTextColor)
            {
                Spacing = 2;
                Orientation = StackOrientation.Horizontal;

                HorizontalOptions = LayoutOptions.Center;

                Children.Add(new AboutPageSvgImage(svgFileName, getTextColor));
                Children.Add(new StatsTitleLabel(text));
            }

            class AboutPageSvgImage : SvgImage
            {
                public AboutPageSvgImage(in string svgFileName, in Func<Color> getTextColor) : base(svgFileName, getTextColor, 12, 12)
                {
                    HorizontalOptions = LayoutOptions.End;
                    VerticalOptions = LayoutOptions.Center;
                }
            }

            class StatsTitleLabel : Label
            {
                public StatsTitleLabel(in string text)
                {
                    Text = text;
                    FontSize = 12;
                    FontFamily = FontFamilyConstants.RobotoMedium;

                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    VerticalOptions = LayoutOptions.FillAndExpand;

                    HorizontalTextAlignment = TextAlignment.Start;
                    VerticalTextAlignment = TextAlignment.Center;

                    this.DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
                }
            }
        }
    }
}