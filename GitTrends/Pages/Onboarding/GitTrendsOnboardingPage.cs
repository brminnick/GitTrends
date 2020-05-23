using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class GitTrendsOnboardingPage : BaseOnboardingContentPage
    {
        public GitTrendsOnboardingPage(IAnalyticsService analyticsService, IMainThread mainThread) : base(analyticsService, mainThread, Color.FromHex(BaseTheme.LightTealColorHex), OnboardingConstants.SkipText, 0)
        {
        }

        enum Row { Title, Connect, MonitorImage, MonitorDescription, Discover }
        enum Column { Image, Description }

        protected override View CreateImageView() => new Image
        {
            Source = "GitTrendsWhite",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        protected override TitleLabel CreateDescriptionTitleLabel() => new TitleLabel(OnboardingConstants.GitTrendsPageTitle);

        protected override View CreateDescriptionBodyView() => new Grid
        {
            RowSpacing = 14,

            RowDefinitions = Rows.Define(
                (Row.Title, AbsoluteGridLength(20)),
                (Row.Connect, AbsoluteGridLength(24)),
                (Row.MonitorImage, AbsoluteGridLength(24)),
                (Row.MonitorDescription, AbsoluteGridLength(2)),
                (Row.Discover, AbsoluteGridLength(24))),

            ColumnDefinitions = Columns.Define(
                (Column.Image, AbsoluteGridLength(56)),
                (Column.Description, Star)),

            Children =
            {
                new BodyLabel("GitTrends helps monitor your GitHub repos:").Row(Row.Title).ColumnSpan(All<Column>()),

                new GitHubLogoLabel().Row(Row.Connect).Column(Column.Image),
                new BodyLabel("Connect to Github").Row(Row.Connect).Column(Column.Description),

                new BodySvg("chart.svg").Row(Row.MonitorImage).Column(Column.Image).Center().RowSpan(2),
                new BodyLabel("Monitor Github Repo Views, Clones, Forks, Stars and Issues"){ VerticalTextAlignment = TextAlignment.Start }.Row(Row.MonitorImage).RowSpan(2).Column(Column.Description),

                new BodySvg("megaphone.svg").Row(Row.Discover).Column(Column.Image),
                new BodyLabel("Discover Referring Sites").Row(Row.Discover).Column(Column.Description),
            }
        };

        class GitHubLogoLabel : Label
        {
            public GitHubLogoLabel()
            {
                Text = FontAwesomeBrandsConstants.GitHubOctocat;
                FontSize = 24;
                TextColor = Color.White;
                FontFamily = FontFamilyConstants.FontAwesomeBrands;
                VerticalTextAlignment = TextAlignment.Center;
                HorizontalTextAlignment = TextAlignment.Center;
            }
        }
    }
}
