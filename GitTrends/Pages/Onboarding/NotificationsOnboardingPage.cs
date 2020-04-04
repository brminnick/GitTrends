using GitTrends.Mobile.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class NotificationsOnboardingPage : BaseOnboardingContentPage
    {
        public NotificationsOnboardingPage(GitHubAuthenticationService gitHubAuthenticationService)
            : base(gitHubAuthenticationService, TealBackgroundColorHex, OnboardingConstants.SkipText, 2)
        {
        }

        protected override View CreateImageView() => new Image
        {
            Source = "NotificationsOnboarding",
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            VerticalOptions = LayoutOptions.CenterAndExpand,
            Aspect = Aspect.AspectFit
        };

        protected override TitleLabel CreateDescriptionTitleLabel() => new TitleLabel("Enable notifications to stay in the loop");

        protected override View CreateDescriptionBodyView() => new Grid
        {
            RowSpacing = 14,

            RowDefinitions = Rows.Define(
                (Row.Description, AbsoluteGridLength(65)),
                (Row.Button, AbsoluteGridLength(42))),

            Children =
            {
                new BodyLabel("GitTrends will notify you when your GitHub repositories are receiving more traffic than usual.").Row(Row.Description),
                new EnableNotificationsView().Row(Row.Button)
            }
        };

        class EnableNotificationsView : PancakeView
        {
            public EnableNotificationsView()
            {
                AutomationId = OnboardingAutomationIds.EnableNotificationsButton;
                HorizontalOptions = LayoutOptions.CenterAndExpand;
                VerticalOptions = LayoutOptions.CenterAndExpand;
                Padding = new Thickness(10);
                BackgroundColor = Color.FromHex(CoralBackgroundColorHex);
                CornerRadius = 5;

                Content = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children =
                    {
                        new BellSvgImage(),
                        new EnableNotificationsLabel()
                    }
                };
            }

            class BellSvgImage : SvgImage
            {
                public BellSvgImage() : base("bell.svg", () => Color.White, 24, 24)
                {
                }
            }

            class EnableNotificationsLabel : Label
            {
                public EnableNotificationsLabel()
                {
                    HorizontalTextAlignment = TextAlignment.Center;
                    VerticalTextAlignment = TextAlignment.Center;
                    TextColor = Color.White;
                    FontSize = 18;
                    FontFamily = FontFamilyConstants.RobotoRegular;
                    Text = "Enable Notifications";
                }
            }
        }

        enum Row { Description, Button }
    }
}
