using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using static GitTrends.XamarinFormsService;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    public class NotificationsOnboardingPage : BaseOnboardingContentPage
    {
        public NotificationsOnboardingPage(IAnalyticsService analyticsService, IMainThread mainThread) : base(analyticsService, mainThread, Color.FromHex(BaseTheme.LightTealColorHex), OnboardingConstants.SkipText, 2)
        {

        }

        enum Row { Description, Button }

        protected override View CreateImageView() => new Image
        {
            Source = "NotificationsOnboarding",
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            VerticalOptions = LayoutOptions.CenterAndExpand,
            Aspect = Aspect.AspectFit
        };

        protected override TitleLabel CreateDescriptionTitleLabel() => new TitleLabel(OnboardingConstants.NotificationsPageTitle);

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
                this.CenterExpand();

                AutomationId = OnboardingAutomationIds.EnableNotificationsButton;

                BackgroundColor = Color.FromHex(BaseTheme.CoralColorHex);
                CornerRadius = 5;
                Padding = new Thickness(16, 10);

                Content = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 16,
                    Children =
                    {
                        new NotificationStatusSvgImage(),
                        new EnableNotificationsLabel()
                    }
                };

                this.BindTapGesture(nameof(OnboardingViewModel.EnableNotificationsButtonTapped));
            }

            class NotificationStatusSvgImage : SvgImage
            {
                public NotificationStatusSvgImage() : base("bell.svg", () => Color.White, 24, 24)
                {
                    this.Bind(SvgImage.SourceProperty, nameof(OnboardingViewModel.NotificationStatusSvgImageSource));
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
    }
}
