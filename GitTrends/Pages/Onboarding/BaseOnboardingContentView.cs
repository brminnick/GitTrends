using GitTrends.Mobile.Common;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using static GitTrends.MauiService;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

public abstract class BaseOnboardingContentView : Grid
{
	protected BaseOnboardingContentView(in string nextButtonText,
		in IDeviceInfo deviceInfo,
		in Color backgroundColor,
		in int carouselPositionIndex,
		in IAnalyticsService analyticsService)
	{
		AnalyticsService = analyticsService;
		DeviceInfo = deviceInfo;

		//Don't Use BaseTheme.PageBackgroundColor
		RemoveDynamicResource(BackgroundColorProperty);

		AutomationId = OnboardingAutomationIds.OnboardingPage;
		BackgroundColor = backgroundColor;


		RowDefinitions = Rows.Define(
			(Row.Image, Stars(GetImageRowStarHeight())),
			(Row.Description, Stars(GetDescriptionRowStarHeight())),
			(Row.Indicator, 44));

		ColumnDefinitions = Columns.Define(
			(Column.Indicator, Stars(1)),
			(Column.Button, Stars(1)));

		Children.Add(new OpacityOverlay()
			.Row(Row.Image).ColumnSpan(All<Column>()));

		Children.Add(CreateImageView()
			.Row(Row.Image).ColumnSpan(All<Column>()).Margin(deviceInfo.Platform == DevicePlatform.iOS ? new Thickness(32, 44 + 32, 32, 32) : new Thickness(32, 16)));

		Children.Add(new VerticalStackLayout
			{
				Spacing = 16,
				Children =
				{
					CreateDescriptionTitleLabel(),
					CreateDescriptionBodyView()
				}
			}.Margin(32, 8)
			.Row(Row.Description)
			.RowSpan(2).ColumnSpan(All<Column>()));

		Children.Add(new OnboardingIndicatorView(carouselPositionIndex)
			.Row(Row.Indicator).Column(Column.Indicator));

		Children.Add(new NextLabel(nextButtonText)
			.Row(Row.Indicator).Column(Column.Button));
	}

	enum Row { Image, Description, Indicator }
	enum Column { Indicator, Button }

	protected abstract View CreateImageView();
	protected abstract TitleLabel CreateDescriptionTitleLabel();
	protected abstract View CreateDescriptionBodyView();

	protected IDeviceInfo DeviceInfo { get; }
	protected IAnalyticsService AnalyticsService { get; }

	int GetImageRowStarHeight()
	{
		if (ScreenHeight < 700)
			return 8;

		return DeviceInfo.Platform == DevicePlatform.iOS ? 3 : 11;
	}

	int GetDescriptionRowStarHeight()
	{
		if (ScreenHeight < 700)
			return 9;

		return DeviceInfo.Platform == DevicePlatform.iOS ? 2 : 9;
	}

	protected sealed class BodySvg(in string svgFileName) : SvgImage(svgFileName, () => Colors.White, 24, 24);

	protected sealed class TitleLabel : Label
	{
		public TitleLabel(in string text)
		{
			Text = text;
			FontSize = 34;
			TextColor = Colors.White;
			LineHeight = 1.12;
			FontFamily = FontFamilyConstants.RobotoBold;
			AutomationId = OnboardingAutomationIds.TitleLabel;
		}
	}

	protected sealed class BodyLabel : Label
	{
		public BodyLabel(in string text)
		{
			Text = text;
			FontSize = 15;
			TextColor = Colors.White;
			LineHeight = 1.021;
			LineBreakMode = LineBreakMode.WordWrap;
			FontFamily = FontFamilyConstants.RobotoRegular;
			VerticalTextAlignment = TextAlignment.Start;
		}
	}

	sealed class NextLabel : Label
	{
		public NextLabel(in string text)
		{
			Text = text;
			FontSize = 15;
			TextColor = Colors.White;
			BackgroundColor = Colors.Transparent;
			FontFamily = FontFamilyConstants.RobotoRegular;

			Opacity = 0.8;

			Margin = new Thickness(0, 0, 30, 0);

			HorizontalOptions = LayoutOptions.End;
			VerticalOptions = LayoutOptions.Center;

			AutomationId = OnboardingAutomationIds.NextButon;

			GestureRecognizers.Add(new TapGestureRecognizer
				{
					CommandParameter = text
				}
				.Bind(TapGestureRecognizer.CommandProperty, nameof(OnboardingViewModel.HandleDemoButtonTappedCommand)));

			this.SetBinding(IsVisibleProperty, nameof(OnboardingViewModel.IsDemoButtonVisible));
		}
	}

	sealed class OpacityOverlay : View
	{
		public OpacityOverlay() => BackgroundColor = Colors.White.MultiplyAlpha(0.25f);
	}

	sealed class OnboardingIndicatorView : IndicatorView
	{
		public OnboardingIndicatorView(in int position)
		{
			Position = position;

			IsEnabled = false;

			SelectedIndicatorColor = Colors.White;
			IndicatorColor = Colors.White.MultiplyAlpha(0.25f);

			Margin = new Thickness(30, 0, 0, 0);

			HorizontalOptions = LayoutOptions.Start;
			AutomationId = OnboardingAutomationIds.PageIndicator;

			SetBinding(CountProperty, new Binding(nameof(OnboardingPage.PageCount),
				source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(OnboardingPage))));
		}
	}
}