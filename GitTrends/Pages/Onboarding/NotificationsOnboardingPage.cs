using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends
{
	public class NotificationsOnboardingPage(IDeviceInfo deviceInfo, IAnalyticsService analyticsService, MediaElementService mediaElementService) 
		: BaseOnboardingContentPage(OnboardingConstants.SkipText, deviceInfo, Color.FromArgb(BaseTheme.LightTealColorHex), 2, analyticsService, mediaElementService)
	{

		enum Row { Description, Button }

		protected override View CreateImageView() => new Image
		{
			Source = "NotificationsOnboarding",
			Aspect = Aspect.AspectFit
		}.Center();

		protected override TitleLabel CreateDescriptionTitleLabel() => new(OnboardingConstants.NotificationsPage_Title);

		protected override View CreateDescriptionBodyView() => new ScrollView
		{
			Content = new Grid
			{
				RowSpacing = 14,

				RowDefinitions = Rows.Define(
					(Row.Description, 65),
					(Row.Button, 42)),

				Children =
				{
					new BodyLabel(OnboardingConstants.NotificationsPage_Body_MoreTrafficThanUsual).Row(Row.Description),
					new EnableNotificationsView().Row(Row.Button)
				}
			}
		};

		sealed class EnableNotificationsView : Border
		{
			public EnableNotificationsView()
			{
				this.Center();

				AutomationId = OnboardingAutomationIds.EnableNotificationsButton;

				BackgroundColor = Color.FromArgb(BaseTheme.CoralColorHex);

				StrokeShape = new RoundRectangle
				{
					CornerRadius = 5
				};
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

				this.BindTapGesture(nameof(OnboardingViewModel.HandleEnableNotificationsButtonTappedCommand));
			}

			class NotificationStatusSvgImage : SvgImage
			{
				public NotificationStatusSvgImage() : base("bell.svg", () => Colors.White, 24, 24)
				{
					this.Bind(SvgImage.SourceProperty, nameof(OnboardingViewModel.NotificationStatusSvgImageSource), BindingMode.OneWay);
				}
			}

			class EnableNotificationsLabel : Label
			{
				public EnableNotificationsLabel()
				{
					this.TextCenter();
					TextColor = Colors.White;
					FontSize = 18;
					FontFamily = FontFamilyConstants.RobotoRegular;
					Text = OnboardingConstants.EnableNotifications;
				}
			}
		}
	}
}