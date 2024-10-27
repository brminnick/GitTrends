using AsyncAwaitBestPractices;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Resources;
using Microsoft.Maui.Controls.Shapes;
using Sharpnado.MaterialFrame;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

class OrganizationsCarouselOverlay : Grid
{
	readonly IAnalyticsService _analyticsService;

	public OrganizationsCarouselOverlay(IDeviceInfo deviceInfo, IAnalyticsService analyticsService)
	{
		_analyticsService = analyticsService;
		InputTransparent = false;

		RowDefinitions = Rows.Define(
			(Row.CloseButton, Star),
			(Row.CarouselFrame, Stars(8)),
			(Row.BottomPadding, Star));

		ColumnDefinitions = Columns.Define(
			(Column.Left, Star),
			(Column.Center, Stars(8)),
			(Column.Right, Star));

		Children.Add(new BackgroundOverlay()
			.RowSpan(All<Row>()).ColumnSpan(All<Column>()));

		Children.Add(new CloseButton(() => Dismiss(true), analyticsService)
			.Row(Row.CloseButton).Column(Column.Right));

		Children.Add(new OrganizationsCarouselFrame(deviceInfo, analyticsService)
			.Row(Row.CarouselFrame).Column(Column.Center));

		Dismiss(false).SafeFireAndForget(ex => analyticsService.Report(ex));
	}

	enum Row { CloseButton, CarouselFrame, BottomPadding }
	enum Column { Left, Center, Right }

	public Task Reveal(bool shouldAnimate) => Dispatcher.DispatchAsync(async () =>
	{
		if (shouldAnimate)
			await this.FadeTo(1);
		else
			Opacity = 1;

		InputTransparent = false;

		_analyticsService.Track($"{nameof(OrganizationsCarouselOverlay)} Appeared");
	});

	public Task Dismiss(bool shouldAnimate) => Dispatcher.DispatchAsync(async () =>
	{
		InputTransparent = true;

		if (shouldAnimate)
			await this.FadeTo(0, 1000);
		else
			Opacity = 0;

		_analyticsService.Track($"{nameof(OrganizationsCarouselOverlay)} Dismissed");
	});

	sealed class BackgroundOverlay : BoxView
	{
		public BackgroundOverlay()
		{
			InputTransparent = false;
			SetBackgroundColor();
			ThemeService.PreferenceChanged += HandlePreferenceChanged;
		}

		void HandlePreferenceChanged(object? sender, PreferredTheme e) => SetBackgroundColor();

		void SetBackgroundColor()
		{
			var pageBackgroundColor = AppResources.GetResource<Color>(nameof(BaseTheme.PageBackgroundColor));
			BackgroundColor = pageBackgroundColor.MultiplyAlpha(0.75f);
		}
	}

	sealed class CloseButton : Label
	{
		readonly IAnalyticsService _analyticsService;
		readonly Func<Task> _onOverlayDismissed;

		public CloseButton(Func<Task> onOverlayDismissed, IAnalyticsService analyticsService)
		{
			Text = "x";
			_analyticsService = analyticsService;
			_onOverlayDismissed = onOverlayDismissed;

			GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new AsyncRelayCommand(OnCloseButtonCommand),
				NumberOfTapsRequired = 1
			});

			BackgroundColor = Colors.Transparent;

			FontSize = 24;
			FontFamily = FontFamilyConstants.RobotoBold;

			this.DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
		}

		async Task OnCloseButtonCommand()
		{
			IsEnabled = false;

			_analyticsService.Track($"{nameof(OrganizationsCarouselOverlay)} Close Button Tapped");

			// Make the button disappear before OrganizationsCarouselFrame
			await Task.WhenAll(_onOverlayDismissed(), this.FadeTo(0));

			//Ensure the Button is visible and reenabled when it next appears
			Opacity = 1;
			IsEnabled = true;
		}
	}

	sealed class OrganizationsCarouselFrame : MaterialFrame
	{
		const int _cornerRadius = 12;

		readonly IndicatorView _indicatorView;
		readonly IAnalyticsService _analyticsService;

		public OrganizationsCarouselFrame(IDeviceInfo deviceInfo, IAnalyticsService analyticsService)
		{
			_analyticsService = analyticsService;

			Padding = 0;

			CornerRadius = _cornerRadius;

			LightThemeBackgroundColor = GetBackgroundColor(0);

			Elevation = 8;

			Content = new EnableOrganizationsGrid
			{
				Padding = 0,
				IsClippedToBounds = true,

				Children =
				{
					new OpacityOverlay()
						.Row(EnableOrganizationsGrid.Row.Image)
						.Fill(),

					new OrganizationsCarouselView(deviceInfo, analyticsService)
						.RowSpan(All<EnableOrganizationsGrid.Row>())
						.Invoke(view => view.PositionChanged += HandlePositionChanged)
						.Fill(),

					new EnableOrganizationsCarouselIndicatorView()
						.Row(EnableOrganizationsGrid.Row.IndicatorView)
						.Assign(out _indicatorView)
				}
			};
		}

		static Color GetBackgroundColor(int position) => position % 2 is 0
			? Color.FromArgb(BaseTheme.LightTealColorHex) // Even-numbered Pages are Teal
			: Color.FromArgb(BaseTheme.CoralColorHex); // Odd-numbered Pages are Coral

		void HandlePositionChanged(object? sender, PositionChangedEventArgs e)
		{
			LightThemeBackgroundColor = GetBackgroundColor(e.CurrentPosition);
			_indicatorView.Position = e.CurrentPosition;

			_analyticsService.Track($"{nameof(OrganizationsCarouselView)} Page {e.CurrentPosition} Appeared");
		}

		sealed class OpacityOverlay : Border
		{
			public OpacityOverlay()
			{
				Padding = Margin = 0;
				BackgroundColor = Colors.White.MultiplyAlpha(0.25f);
				StrokeShape = new RoundRectangle
				{
					CornerRadius = new CornerRadius(_cornerRadius, _cornerRadius, 0, 0)
				};
			}
		}

		sealed class OrganizationsCarouselView : BaseCarouselView
		{
			public OrganizationsCarouselView(IDeviceInfo deviceInfo, IAnalyticsService analyticsService) : base(analyticsService)
			{
				HorizontalScrollBarVisibility = ScrollBarVisibility.Never;

				ItemsSource = new List<IncludeOrganizationsCarouselModel>(3)
				{
					new(ManageOrganizationsConstants.GitHubOrganizationsTitle, ManageOrganizationsConstants.GitHubOrganizationsDescription, 0, "Business", null),
					new(ManageOrganizationsConstants.GitTrendsAccessTitle, ManageOrganizationsConstants.GitTrendsAccessDescription, 1, "Inspectocat", null),
					new(ManageOrganizationsConstants.EnableOrganizationsTitle, ManageOrganizationsConstants.EnableOrganizationsDescription, 2, null, VideoConstants.EnableOrganizationsFileName),
				};

				ItemTemplate = new EnableOrganizationsCarouselTemplate(deviceInfo, this);
			}

			protected override void OnPositionChanged(PositionChangedEventArgs args)
			{
				base.OnPositionChanged(args);

				AnalyticsService.Track($"{GetType().Name} Page {args.CurrentPosition} Appeared");
			}
		}

		sealed class EnableOrganizationsCarouselIndicatorView : IndicatorView
		{
			public EnableOrganizationsCarouselIndicatorView()
			{
				InputTransparent = true;

				Count = 3;
				IsEnabled = false;
				SelectedIndicatorColor = Colors.White;
				IndicatorColor = SelectedIndicatorColor.MultiplyAlpha(0.25f);

				IndicatorSize = 12;

				AutomationId = SettingsPageAutomationIds.EnableOrangizationsPageIndicator;

				this.Center().Margins(bottom: 12);
			}
		}
	}
}