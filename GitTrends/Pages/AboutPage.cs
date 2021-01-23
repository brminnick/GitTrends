using System.Linq;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Markup;
using static GitTrends.MarkupExtensions;
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
            _deepLinkingService = deepLinkingService;

            Title = AboutPageConstants.About;

            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(
                    (Column.Icon, Stars(4)),
                    (Column.WatchingIcon, AbsoluteGridLength(5)),
                    (Column.WatchingTitle, Stars(2)),
                    (Column.WatchingSeparator, AbsoluteGridLength(1)),
                    (Column.StarsIcon, AbsoluteGridLength(5)),
                    (Column.StarsTitle, Stars(2)),
                    (Column.StarsSeparator, AbsoluteGridLength(1)),
                    (Column.ForksIcon, AbsoluteGridLength(5)),
                    (Column.ForksTitle, Stars(2))),

                RowDefinitions = Rows.Define(
                    (Row.Title, AbsoluteGridLength(30)),
                    (Row.Description, AbsoluteGridLength(50)),
                    (Row.StatsTitle, AbsoluteGridLength(10)),
                    (Row.StatsNumber, AbsoluteGridLength(20)),
                    (Row.ActionButtons, AbsoluteGridLength(35)),
                    (Row.CollaboratorTitle, AbsoluteGridLength(10)),
                    (Row.CollaboratorDescription, AbsoluteGridLength(25)),
                    (Row.CollaboratorCollection, AbsoluteGridLength(50)),
                    (Row.LibrariesTitle, AbsoluteGridLength(10)),
                    (Row.LibrariesDescription, AbsoluteGridLength(15)),
                    (Row.LibrariesCollection, Stars(1))),

                Children =
                {
                    new Label { BackgroundColor = Color.Accent, Text = "GitTrends Icon"}
                        .Row(Row.Title).RowSpan(4).Column(Column.Icon),

                    new Label { BackgroundColor = Color.AliceBlue, Text = "GitTrends"}
                        .Row(Row.Title).Column(Column.WatchingIcon).ColumnSpan(8),

                    new Label { BackgroundColor = Color.AntiqueWhite, Text = "Description"}
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

                    new Label { BackgroundColor = Color.CadetBlue, Text = "Collaborator Collection" }
                        .Row(Row.CollaboratorCollection).ColumnSpan(All<Column>()),

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


/*        new CollectionView
                        {
                            SelectionMode = SelectionMode.Single,
                            ItemTemplate = new ContributorDataTemplate(),
                            ItemsSource = ViewModel.GitTrendsContributors,
                            ItemsLayout = new GridItemsLayout(IsSmallScreen? 4: 5, ItemsLayoutOrientation.Vertical)
                        }.Invoke(collectionView => collectionView.SelectionChanged += HandleContributorSelectionChanged)*/



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
            const int _circleDiameter = 50;

            public ContributorDataTemplate() : base(CreateContributorDataTemplate)
            {

            }

            static Grid CreateContributorDataTemplate() => new Grid
            {
                RowSpacing = 4,

                RowDefinitions = Rows.Define(
                    (Row.Avatar, AbsoluteGridLength(_circleDiameter)),
                    (Row.Login, AbsoluteGridLength(25))),

                Children =
                {
                    new AvatarImage(_circleDiameter)
                        .Row(Row.Avatar)
                        .Bind(CircleImage.ImageSourceProperty, nameof(Contributor.AvatarUrl), BindingMode.OneTime)
                        .DynamicResource(CircleImage.BorderColorProperty, nameof(BaseTheme.SeparatorColor)),

                    new Label { LineBreakMode = LineBreakMode.TailTruncation }.TextCenterHorizontal().TextTop().Font(FontFamilyConstants.RobotoRegular, 12)
                        .Row(Row.Login)
                        .Bind(Label.TextProperty, nameof(Contributor.Login), BindingMode.OneTime)
                        .DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
                }
            };

            enum Row { Avatar, Login }
        }
    }
}
