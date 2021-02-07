using System;
using System.Linq;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;
using static GitTrends.XamarinFormsService;
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
            const int horizontalPadding = 28;
            const int separatorColumnWidth = 1;

            _deepLinkingService = deepLinkingService;

            Title = PageTitles.AboutPage;
            Padding = new Thickness(0, 16, 0, 0);

            Content = new Grid
            {
                ColumnSpacing = 2,
                RowSpacing = 6,

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
                    (Row.Title, 32),
                    (Row.Description, 48),
                    (Row.StatsTitle, 16),
                    (Row.StatsNumber, 20),
                    (Row.ActionButtons, 60),
                    (Row.CollaboratorTitle, 20),
                    (Row.CollaboratorDescription, 32),
                    (Row.CollaboratorCollection, 100),
                    (Row.LibrariesTitle, 20),
                    (Row.LibrariesDescription, 32),
                    (Row.LibrariesCollection, Star)),

                Children =
                {
                     new Image().CenterExpand().Margins(right: 12)
                        .Row(Row.Title).RowSpan(4).Column(Column.Icon)
                        .DynamicResource(Image.SourceProperty, nameof(BaseTheme.GitTrendsImageSource)),

                    new Label { Text = "GitTrends" }.Font(FontFamilyConstants.RobotoMedium, 24)
                        .Row(Row.Title).Column(Column.Watching).ColumnSpan(5)
                        .DynamicResource(Label.TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor)),

                    new DescriptionLabel(AboutPageConstants.DescriptionText) { MaxLines = 3 }
                        .Row(Row.Description).Column(Column.Watching).ColumnSpan(5),

                    new StatsTitleLayout(AboutPageConstants.Watching, "unique_views.svg", () => (Color)Application.Current.Resources[nameof(BaseTheme.SettingsLabelTextColor)]).Assign(out StatsTitleLayout watchingTitleLabel)
                        .Row(Row.StatsTitle).Column(Column.Watching),

                    new BoxView()
                        .Row(Row.StatsTitle).RowSpan(2).Column(Column.WatchingSeparator)
                        .DynamicResource(BackgroundColorProperty,nameof(BaseTheme.SettingsLabelTextColor)),

                    new StatsTitleLayout(AboutPageConstants.Stars, "star.svg",() => (Color)Application.Current.Resources[nameof(BaseTheme.SettingsLabelTextColor)]).Assign(out StatsTitleLayout starsTitleLabel)
                        .Row(Row.StatsTitle).Column(Column.Stars),

                    new BoxView()
                        .Row(Row.StatsTitle).RowSpan(2).Column(Column.StarsSeparator)
                        .DynamicResource(BackgroundColorProperty,nameof(BaseTheme.SettingsLabelTextColor)),

                    new StatsTitleLayout(AboutPageConstants.Forks, "repo_forked.svg",() => (Color)Application.Current.Resources[nameof(BaseTheme.SettingsLabelTextColor)]).Assign(out StatsTitleLayout forksTitleLabel)
                        .Row(Row.StatsTitle).Column(Column.Forks),

                    new StatisticsLabel(ViewModel.Watchers?.ToAbbreviatedText() ?? string.Empty, AboutPageAutomationIds.WatchersLabel, watchingTitleLabel)
                        .Row(Row.StatsNumber).Column(Column.Watching),

                    new StatisticsLabel(ViewModel.Stars?.ToAbbreviatedText() ?? string.Empty, AboutPageAutomationIds.StarsLabel, starsTitleLabel)
                        .Row(Row.StatsNumber).Column(Column.Stars),

                    new StatisticsLabel(ViewModel.Forks?.ToAbbreviatedText() ?? string.Empty, AboutPageAutomationIds.ForksLabel, forksTitleLabel)
                        .Row(Row.StatsNumber).Column(Column.Forks),

                    new ButtonLayout().Center()
                        .Row(Row.ActionButtons).Column(Column.Icon).ColumnSpan(6),

                    new TitleLabel(AboutPageConstants.CollaboratorsTitle).TextBottom().BottomExpand()
                        .Row(Row.CollaboratorTitle).Column(Column.Icon).ColumnSpan(6),

                    new DescriptionLabel(AboutPageConstants.CollaboratorsDescription)
                        .Row(Row.CollaboratorDescription).Column(Column.Icon).ColumnSpan(6),

                    new CollectionView
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                        HeightRequest = ContributorDataTemplate.RowHeight + 8,
                        Header = new BoxView { WidthRequest = horizontalPadding },
                        SelectionMode = SelectionMode.Single,
                        ItemTemplate = new ContributorDataTemplate(),
                        ItemsSource = ViewModel.GitTrendsContributors,
                        ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
                    }.Top()
                     .Row(Row.CollaboratorCollection).ColumnSpan(All<Column>())
                     .Invoke(collectionView => collectionView.SelectionChanged += HandleContributorSelectionChanged),

                    new TitleLabel(AboutPageConstants.LibrariesTitle)
                        .Row(Row.LibrariesTitle).Column(Column.Icon).ColumnSpan(6),

                    new DescriptionLabel(AboutPageConstants.LibrariesDescription)
                        .Row(Row.LibrariesDescription).Column(Column.Icon).ColumnSpan(6),

                    new CollectionView
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                        HeightRequest = LibraryDataTemplate.RowHeight * 2 + 8,
                        Header = new BoxView { WidthRequest = horizontalPadding },
                        SelectionMode = SelectionMode.Single,
                        ItemTemplate = new LibraryDataTemplate(),
                        ItemsSource = ViewModel.InstalledLibraries,
                        ItemsLayout = IsSmallScreen ? new LinearItemsLayout(ItemsLayoutOrientation.Horizontal) : new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal)
                    }.Top()
                     .Row(Row.LibrariesCollection).ColumnSpan(All<Column>())
                     .Invoke(collectionView => collectionView.SelectionChanged += HandleLibrarySelectionChanged),
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

        async void HandleLibrarySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;
            collectionView.SelectedItem = null;

            if (e.CurrentSelection.FirstOrDefault() is NuGetPackageModel nuGetPackageModel)
            {
                AnalyticsService.Track("Library Tapped", nameof(NuGetPackageModel.PackageName), nuGetPackageModel.PackageName);

                await _deepLinkingService.OpenBrowser(nuGetPackageModel.WebsiteUri);
            }
        }

        class StatisticsLabel : Label
        {
            public StatisticsLabel(in string text, in string automationId, in StatsTitleLayout statsTitleLayout)
            {
                Text = text;
                AutomationId = automationId;

                FontSize = 16;
                FontFamily = FontFamilyConstants.RobotoRegular;

                this.Center().TextCenter().DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
                this.Bind(WidthRequestProperty, nameof(Width), source: statsTitleLayout);
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

                MaxLines = 2;

                HorizontalOptions = LayoutOptions.StartAndExpand;
                VerticalOptions = LayoutOptions.StartAndExpand;

                FontFamily = FontFamilyConstants.RobotoRegular;
                LineBreakMode = LineBreakMode.TailTruncation;

                this.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor));
            }
        }

        class ButtonLayout : StackLayout
        {
            public ButtonLayout()
            {
                Spacing = 16;
                Orientation = StackOrientation.Horizontal;

                Margin = new Thickness(0, 8, 0, 16);

                Children.Add(new ViewOnGitHubButton().EndExpand());
                Children.Add(new RequestFeatureButton().StartExpand());
            }

            class ViewOnGitHubButton : AboutPageButton
            {
                public ViewOnGitHubButton() : base("github.svg", "View on GitHub", AboutPageAutomationIds.ViewOnGitHubButton, Color.FromHex("231F20"), nameof(AboutViewModel.ViewOnGitHubCommand))
                {
                }
            }

            class RequestFeatureButton : AboutPageButton
            {
                public RequestFeatureButton() : base("sparkle.svg", "Request Feature", AboutPageAutomationIds.RequestFeatureButton, Color.FromHex("F97B4F"), nameof(AboutViewModel.RequestFeatureCommand))
                {
                }
            }


            abstract class AboutPageButton : SvgTextLabel
            {
                protected AboutPageButton(in string svgFileName, in string text, in string automationId, in Color backgroundColor, in string commandPropertyBindingPath)
                    : base(svgFileName, text, automationId, IsSmallScreen ? 10 : 14, FontFamilyConstants.RobotoMedium, 4)
                {
                    BackgroundColor = backgroundColor;
                    Padding = new Thickness(DeviceInfo.Platform == DevicePlatform.iOS ? 12 : 16, 8);
                    GestureRecognizers.Add(new TapGestureRecognizer().Bind(TapGestureRecognizer.CommandProperty, commandPropertyBindingPath));
                }
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