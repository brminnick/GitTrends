using GitTrends.Mobile.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class WelcomePage : BaseOnboardingPage
    {
        public WelcomePage() : base(OnboardingConstants.TealBackgroundColorHex, OnboardingConstants.SkipText)
        {
        }

        protected override View CreateImageView() => new Image
        {
            Source = "GitTrends",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        protected override TitleLabel CreateDescriptionTitleLabel() => new TitleLabel("Welcome to GitTrends");

        protected override View CreateDescriptionBodyView() => new Grid
        {
            RowSpacing = 14,

            RowDefinitions = Rows.Define(
                (Row.Title, AbsoluteGridLength(20)),
                (Row.Row1, AbsoluteGridLength(24)),
                (Row.Row2, AbsoluteGridLength(24)),
                (Row.Row3, AbsoluteGridLength(24))),

            ColumnDefinitions = Columns.Define(
                (Column.Image, AbsoluteGridLength(56)),
                (Column.Description, Star)),

            Children =
            {
                new BodyLabel("GitTrends helps monitor your GitHub repos:").Row(Row.Title).ColumnSpan(All<Column>()),

                new GitHubLogoLabel().Row(Row.Row1).Column(Column.Image),
                new BodyLabel("Connect to Github").Row(Row.Row1).Column(Column.Description),

                new BodySvg("chart.svg").Row(Row.Row2).Column(Column.Image),
                new BodyLabel("Monitor Github Repo Views, Clones, Forks, Stars and Issues").Row(Row.Row2).Column(Column.Description),

                new BodySvg("megaphone.svg").Row(Row.Row3).Column(Column.Image),
                new BodyLabel("Discover Referring Sites").Row(Row.Row3).Column(Column.Description),
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

        enum Row { Title, Row1, Row2, Row3 }
        enum Column { Image, Description }
    }
}
