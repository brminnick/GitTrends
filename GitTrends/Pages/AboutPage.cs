using System.Linq;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Markup;
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
            _deepLinkingService = deepLinkingService;

            Title = AboutPageConstants.About;
            Padding = new Thickness(16, 8);

            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(
                    (Column.Icon, Stars(4)),
                    (Column.WatchingIcon, 5),
                    (Column.WatchingTitle, Stars(2)),
                    (Column.WatchingSeparator, 1),
                    (Column.StarsIcon, 5),
                    (Column.StarsTitle, Stars(2)),
                    (Column.StarsSeparator, 1),
                    (Column.ForksIcon, 5),
                    (Column.ForksTitle, Stars(2))),

                RowDefinitions = Rows.Define(
                    (Row.Title, 30),
                    (Row.Description, 50),
                    (Row.StatsTitle, 10),
                    (Row.StatsNumber, 20),
                    (Row.ActionButtons, 35),
                    (Row.CollaboratorTitle, 10),
                    (Row.CollaboratorDescription, 25),
                    (Row.CollaboratorCollection, 75),
                    (Row.LibrariesTitle, 10),
                    (Row.LibrariesDescription, 15),
                    (Row.LibrariesCollection, Stars(1))),

                Children =
                {
                     new Image().CenterExpand()
                        .Row(Row.Title).RowSpan(4).Column(Column.Icon)
                        .DynamicResource(Image.SourceProperty, nameof(BaseTheme.GitTrendsImageSource)),

                    new Label { Text = "GitTrends" }.Font(FontFamilyConstants.RobotoBold, 36)
                        .Row(Row.Title).Column(Column.WatchingIcon).ColumnSpan(8)
                        .DynamicResource(Label.TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor)),

                    new DescriptionLabel("DescriptionDescriptionDescriptionDescriptionDescriptionDescription")
                        .Row(Row.Description).Column(Column.WatchingIcon).ColumnSpan(8),

                    new Label { BackgroundColor = Color.Aqua, Text = "W" }
                        .Row(Row.StatsTitle).Column(Column.WatchingIcon),

                    new Label { BackgroundColor = Color.Aquamarine, Text = "Watching" }
                        .Row(Row.StatsTitle).Column(Column.WatchingTitle),

                    new Label { BackgroundColor = Color.Azure}
                        .Row(Row.StatsTitle).RowSpan(2).Column(Column.WatchingSeparator),

                    new Label { BackgroundColor = Color.Beige, Text = "S" }
                        .Row(Row.StatsTitle).Column(Column.StarsIcon),

                    new Label { BackgroundColor = Color.Bisque, Text = "Stars" }
                        .Row(Row.StatsTitle).Column(Column.StarsTitle),

                    new Label { BackgroundColor = Color.Black}
                        .Row(Row.StatsTitle).RowSpan(2).Column(Column.StarsSeparator),

                    new Label { BackgroundColor = Color.BlanchedAlmond, Text = "F" }
                        .Row(Row.StatsTitle).Column(Column.ForksIcon),

                    new Label { BackgroundColor = Color.Blue, Text = "Forks" }
                        .Row(Row.StatsTitle).Column(Column.ForksTitle),

                    new Label { BackgroundColor = Color.CornflowerBlue, Text = "Watching Number" }
                        .Row(Row.StatsNumber).Column(Column.WatchingIcon).ColumnSpan(2),

                    new Label { BackgroundColor = Color.Cornsilk, Text = "Stars Number" }
                        .Row(Row.StatsNumber).Column(Column.StarsIcon).ColumnSpan(2),

                    new Label { BackgroundColor = Color.Crimson, Text = "Forks Number" }
                        .Row(Row.StatsNumber).Column(Column.ForksIcon).ColumnSpan(2),

                    new Label { BackgroundColor = Color.BlueViolet, Text = "View on GitHub" }
                        .Row(Row.ActionButtons).Column(Column.Icon).ColumnSpan(3),

                    new Label { BackgroundColor = Color.Brown, Text = "Request Feature" }
                        .Row(Row.ActionButtons).Column(Column.WatchingSeparator).ColumnSpan(6),

                    new Label { BackgroundColor = Color.BurlyWood, Text = "Collaborator" }
                        .Row(Row.CollaboratorTitle).ColumnSpan(All<Column>()),

                    new Label { BackgroundColor = Color.CadetBlue, Text = "Collaborator Description" }
                        .Row(Row.CollaboratorDescription).ColumnSpan(All<Column>()),

                    new CollectionView
                    {
                        SelectionMode = SelectionMode.Single,
                        ItemTemplate = new ContributorDataTemplate(),
                        ItemsSource = ViewModel.GitTrendsContributors,
                        ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
                    }.Row(Row.CollaboratorCollection).ColumnSpan(All<Column>())
                     .Invoke(collectionView => collectionView.SelectionChanged += HandleContributorSelectionChanged),

                    new Label { BackgroundColor = Color.Chartreuse, Text = "Libraries" }
                        .Row(Row.LibrariesTitle).ColumnSpan(All<Column>()),

                    new Label { BackgroundColor = Color.Chocolate, Text = "Library Description" }
                        .Row(Row.LibrariesDescription).ColumnSpan(All<Column>()),

                    new Label { BackgroundColor = Color.Coral, Text = "Library Collection" }
                        .Row(Row.LibrariesCollection).ColumnSpan(All<Column>()),
                }
            };
        }

        enum Row { Title, Description, StatsTitle, StatsNumber, ActionButtons, CollaboratorTitle, CollaboratorDescription, CollaboratorCollection, LibrariesTitle, LibrariesDescription, LibrariesCollection }
        enum Column { Icon, WatchingIcon, WatchingTitle, WatchingSeparator, StarsIcon, StarsTitle, StarsSeparator, ForksIcon, ForksTitle }

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

        class ContributorDataTemplate : DataTemplate
        {
            const int _textPadding = 5;
            const int _circleDiameter = 50;
            const double _horizontalPadding = 0.5;

            public ContributorDataTemplate() : base(CreateContributorDataTemplate)
            {

            }

            enum ContributorRow { Avatar, Login }
            enum ContributorColumn { LeftPadding, LeftText, Image, RightText, RightPadding }

            static Grid CreateContributorDataTemplate() => new()
            {
                RowSpacing = 4,

                RowDefinitions = Rows.Define(
                    (ContributorRow.Avatar, _circleDiameter),
                    (ContributorRow.Login, 25)),

                ColumnDefinitions = Columns.Define(
                    (ContributorColumn.LeftPadding, _horizontalPadding),
                    (ContributorColumn.LeftText, _textPadding),
                    (ContributorColumn.Image, _circleDiameter),
                    (ContributorColumn.RightText, _textPadding),
                    (ContributorColumn.RightPadding, _horizontalPadding)),

                Children =
                {
                    new AvatarImage(_circleDiameter)
                        .Row(ContributorRow.Avatar).Column(ContributorColumn.Image)
                        .Bind(CircleImage.ImageSourceProperty, nameof(Contributor.AvatarUrl), BindingMode.OneTime)
                        .DynamicResource(CircleImage.BorderColorProperty, nameof(BaseTheme.SeparatorColor)),

                    new Label { LineBreakMode = LineBreakMode.TailTruncation }.TextCenterHorizontal().TextTop().Font(FontFamilyConstants.RobotoRegular, 12)
                        .Row(ContributorRow.Login).Column(ContributorColumn.LeftText).ColumnSpan(3)
                        .Bind(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime)
                        .DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
                }
            };
        }

        class TitleLabel : Label
        {
            public TitleLabel()
            {
                FontSize = 14;

                HorizontalOptions = LayoutOptions.StartAndExpand;
                VerticalOptions = LayoutOptions.StartAndExpand;

                FontFamily = FontFamilyConstants.RobotoMedium;
                LineBreakMode = LineBreakMode.TailTruncation;

                this.DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
            }
        }

        class DescriptionLabel : Label
        {
            public DescriptionLabel(string text)
            {
                Text = text;

                FontSize = 10;

                HorizontalOptions = LayoutOptions.StartAndExpand;
                VerticalOptions = LayoutOptions.StartAndExpand;

                FontFamily = FontFamilyConstants.RobotoRegular;
                LineBreakMode = LineBreakMode.TailTruncation;

                this.DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
            }
        }
    }
}