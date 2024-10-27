using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

public class NotificationsOnboardingView(IDeviceInfo deviceInfo, IAnalyticsService analyticsService)
	: BaseOnboardingDataTemplate(
		OnboardingConstants.SkipText,
		deviceInfo,
		Color.FromArgb(BaseTheme.LightTealColorHex),
		2,
		() => new ImageView(),
		() => new(OnboardingConstants.NotificationsPage_Title),
		() => new DescriptionBodyView(deviceInfo),
		analyticsService)
{

	enum Row { Description, Button }

	sealed class ImageView : Image
	{
		public ImageView()
		{

			Source = "NotificationsOnboarding";
			Aspect = Aspect.AspectFit;
			this.Center();
		}
	}

	sealed class DescriptionBodyView : ScrollView
	{
		public DescriptionBodyView(IDeviceInfo deviceInfo)
		{

			Content = new Grid
			{
				RowSpacing = 14,

				RowDefinitions = Rows.Define(
					(Row.Description, 65),
					(Row.Button, 46)),

				Children =
				{
					new BodyLabel(OnboardingConstants.NotificationsPage_Body_MoreTrafficThanUsual).Row(Row.Description),
					new EnableNotificationsView(deviceInfo).Row(Row.Button)
				}
			};
		}
	}

	sealed class EnableNotificationsView : Border
	{
		public EnableNotificationsView(IDeviceInfo deviceInfo)
		{
			this.Center();

			AutomationId = OnboardingAutomationIds.EnableNotificationsButton;

			BackgroundColor = Color.FromArgb(BaseTheme.CoralColorHex);

			StrokeShape = new RoundRectangle
			{
				CornerRadius = 5
			};
			Padding = new Thickness(16, 10);

			Content = new HorizontalStackLayout
			{
				Spacing = 16,
				Children =
				{
					new NotificationStatusSvgImage(deviceInfo),
					new EnableNotificationsLabel()
				}
			};

			this.BindTapGesture(nameof(OnboardingViewModel.HandleEnableNotificationsButtonTappedCommand),
				commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(OnboardingViewModel)));
		}

		sealed class NotificationStatusSvgImage : SvgImage
		{
			public NotificationStatusSvgImage(IDeviceInfo deviceInfo) : base(deviceInfo)
			{
				NotificationService.RegisterForNotificationsCompleted += HandleRegisterForNotificationsCompleted;

				this.Bind(SourceProperty, nameof(OnboardingViewModel.NotificationStatusSvgImageSource), BindingMode.OneWay,
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(OnboardingViewModel)));
			}

			// Work-around for Source PropertyChanged event not firing on iOS
			async void HandleRegisterForNotificationsCompleted(object? sender, (bool IsSuccessful, string ErrorMessage) e)
			{
				await Dispatcher.DispatchAsync(() =>
					Source = e.IsSuccessful ? OnboardingViewModel.SuccessSvgSourceName : OnboardingViewModel.FailSvgSourceName);
			}
		}

		sealed class EnableNotificationsLabel : Label
		{
			public EnableNotificationsLabel()
			{
				this.Center();
				this.TextCenter();

				TextColor = Colors.White;
				FontSize = 18;
				FontFamily = FontFamilyConstants.RobotoRegular;
				Text = OnboardingConstants.EnableNotifications;
			}
		}
	}
}