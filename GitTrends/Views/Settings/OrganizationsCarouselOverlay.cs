using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Sharpnado.MaterialFrame;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends;

class OrganizationsCarouselOverlay : Grid
{
	readonly IMainThread _mainThread;
	readonly IAnalyticsService _analyticsService;

	public OrganizationsCarouselOverlay(IMainThread mainThread,
											IAnalyticsService analyticsService,
											MediaElementService mediaElementService)
	{
		_mainThread = mainThread;
		_analyticsService = analyticsService;

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


		Children.Add(new OrganizationsCarouselFrame(analyticsService, mediaElementService)
						.Row(Row.CarouselFrame).Column(Column.Center));

		Dismiss(false).SafeFireAndForget(ex => analyticsService.Report(ex));
	}

	enum Row { CloseButton, CarouselFrame, BottomPadding }
	enum Column { Left, Center, Right }

	public Task Reveal(bool shouldAnimate) => _mainThread.InvokeOnMainThreadAsync(async () =>
	{
		if (shouldAnimate)
			await this.FadeTo(1);
		else
			Opacity = 1;

		InputTransparent = false;

		_analyticsService.Track($"{nameof(OrganizationsCarouselOverlay)} Appeared");
	});

	public Task Dismiss(bool shouldAnimate) => _mainThread.InvokeOnMainThreadAsync(async () =>
	{
		InputTransparent = true;

		if (shouldAnimate)
			await this.FadeTo(0, 1000);
		else
			Opacity = 0;

		_analyticsService.Track($"{nameof(OrganizationsCarouselOverlay)} Dismissed");
	});

	class BackgroundOverlay : BoxView
	{
		public BackgroundOverlay()
		{
			SetBackgroundColor();
			ThemeService.PreferenceChanged += HandlePreferenceChanged;
		}

		void HandlePreferenceChanged(object sender, PreferredTheme e) => SetBackgroundColor();

		void SetBackgroundColor()
		{
			var pageBackgroundColor = (Color)Application.Current.Resources[nameof(BaseTheme.PageBackgroundColor)];
			BackgroundColor = pageBackgroundColor.MultiplyAlpha(0.75);
		}
	}

	class CloseButton : BounceButton
	{
		public CloseButton(Func<Task> dismissOverlay, IAnalyticsService analyticsService)
		{
			Text = "x";
			Command = new AsyncRelayCommand(async () =>
			{
				IsEnabled = false;

				analyticsService.Track($"{nameof(OrganizationsCarouselOverlay)} Close Button Tapped");

				// Make the button disappear before OrganizationsCarouselFrame
				await Task.WhenAll(dismissOverlay(), this.FadeTo(0));

				//Ensure the Button is visible and reenabled when it next appears
				Opacity = 1;
				IsEnabled = true;
			});

			BackgroundColor = Color.Transparent;

			FontSize = 24;
			FontFamily = FontFamilyConstants.RobotoBold;

			this.DynamicResource(TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor));
		}
	}

	class OrganizationsCarouselFrame : MaterialFrame
	{
		const int _cornerRadius = 12;

		readonly IndicatorView _indicatorView;
		readonly IAnalyticsService _analyticsService;

		public OrganizationsCarouselFrame(IAnalyticsService analyticsService,
											MediaElementService mediaElementService)
		{
			_analyticsService = analyticsService;

			Padding = 0;

			CornerRadius = _cornerRadius;

			LightThemeBackgroundColor = GetBackgroundColor(0);

			Elevation = 8;

			Content = new EnableOrganizationsGrid
			{
				IsClippedToBounds = true,

				Children =
				{
					new OpacityOverlay()
						.Row(EnableOrganizationsGrid.Row.Image),

					new OrganizationsCarouselView(analyticsService, mediaElementService)
						.Row(EnableOrganizationsGrid.Row.Image).RowSpan(All<EnableOrganizationsGrid.Row>())
						.Invoke(view => view.PositionChanged += HandlePositionChanged)
						.FillExpand(),

					new EnableOrganizationsCarouselIndicatorView()
						.Row(EnableOrganizationsGrid.Row.IndicatorView)
						.Assign(out _indicatorView)
				}
			};
		}

		static Color GetBackgroundColor(int position) => position % 2 is 0
															? Color.FromHex(BaseTheme.LightTealColorHex) // Even-numbered Pages are Teal
															: Color.FromHex(BaseTheme.CoralColorHex); // Odd-numbered Pages are Coral

		void HandlePositionChanged(object sender, PositionChangedEventArgs e)
		{
			LightThemeBackgroundColor = GetBackgroundColor(e.CurrentPosition);
			_indicatorView.Position = e.CurrentPosition;

			_analyticsService.Track($"{nameof(OrganizationsCarouselView)} Page {e.CurrentPosition} Appeared");
		}

		class OpacityOverlay : PancakeView
		{
			public OpacityOverlay()
			{
				BackgroundColor = Color.White.MultiplyAlpha(0.25);
				CornerRadius = new CornerRadius(_cornerRadius, _cornerRadius, 0, 0);
			}
		}

		class OrganizationsCarouselView : CarouselView
		{
			readonly IAnalyticsService _analyticsService;

			public OrganizationsCarouselView(IAnalyticsService analyticsService, MediaElementService mediaElementService)
			{
				_analyticsService = analyticsService;

				Loop = false;
				HorizontalScrollBarVisibility = ScrollBarVisibility.Never;

				ItemsSource = new[]
				{
					new IncludeOrganizationsCarouselModel(ManageOrganizationsConstants.GitHubOrganizationsTitle, ManageOrganizationsConstants.GitHubOrganizationsDescription, 0, "Business", null),
					new IncludeOrganizationsCarouselModel(ManageOrganizationsConstants.GitTrendsAccessTitle, ManageOrganizationsConstants.GitTrendsAccessDescription, 1, "Inspectocat", null),
					new IncludeOrganizationsCarouselModel(ManageOrganizationsConstants.EnableOrganizationsTitle, ManageOrganizationsConstants.EnableOrganizationsDescription, 2, null, mediaElementService.EnableOrganizationsUrl),
				};

				ItemTemplate = new EnableOrganizationsCarouselTemplateSelector();
			}

			protected override void OnPositionChanged(PositionChangedEventArgs args)
			{
				base.OnPositionChanged(args);

				_analyticsService.Track($"{GetType().Name} Page {args.CurrentPosition} Appeared");
			}
		}

		class EnableOrganizationsCarouselIndicatorView : IndicatorView
		{
			public EnableOrganizationsCarouselIndicatorView()
			{
				Count = 3;
				IsEnabled = false;
				SelectedIndicatorColor = Color.White;
				IndicatorColor = SelectedIndicatorColor.MultiplyAlpha(0.25);

				WidthRequest = 112;
				HeightRequest = 50;

				IndicatorSize = 12;

				AutomationId = SettingsPageAutomationIds.EnableOrangizationsPageIndicator;

				this.Center().Margins(bottom: 12);
			}
		}
	}
}