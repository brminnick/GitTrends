using CommunityToolkit.Maui.Markup;
using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Resources;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

public sealed class SettingsPage : BaseContentPage<SettingsViewModel>, IDisposable
{
	readonly Grid _contentGrid;
	readonly OrganizationsCarouselOverlay _organizationsCarouselOverlay;

	CancellationTokenSource _connectToGitHubCancellationTokenSource = new();

	public SettingsPage(
		IDeviceInfo deviceInfo,
		IAnalyticsService analyticsService,
		SettingsViewModel settingsViewModel)
		: base(settingsViewModel, analyticsService)
	{
		const int separatorRowHeight = 1;
		const int settingsRowHeight = 38;

		SettingsViewModel.OrganizationsCarouselViewVisibilityChanged += HandleOrganizationsCarouselViewVisibilityChanged;

		var loginRowTapGesture = new TapGestureRecognizer();
		loginRowTapGesture.Tapped += HandleLoginRowTapped;

		var aboutRowTapGesture = new TapGestureRecognizer();
		aboutRowTapGesture.Tapped += HandleAboutRowTapped;

		Content = new Grid
		{
			Children =
			{
				new ScrollView
				{
					Content = new Grid
					{
						RowSpacing = 8,
						ColumnSpacing = 16.5,

						RowDefinitions = Rows.Define(
							(SettingsRow.GitHubUser, GitHubUserView.TotalHeight),
							(SettingsRow.GitHubUserSeparator, separatorRowHeight),
							(SettingsRow.About, settingsRowHeight),
							(SettingsRow.AboutSeparator, separatorRowHeight),
							(SettingsRow.Login, settingsRowHeight),
							(SettingsRow.LoginSeparator, separatorRowHeight),
							(SettingsRow.Organizations, settingsRowHeight),
							(SettingsRow.OrganizationsSeparator, separatorRowHeight),
							(SettingsRow.Notifications, settingsRowHeight),
							(SettingsRow.NotificationsSeparator, separatorRowHeight),
							(SettingsRow.Theme, settingsRowHeight),
							(SettingsRow.ThemeSeparator, separatorRowHeight),
							(SettingsRow.Language, settingsRowHeight),
							(SettingsRow.LanguageSeparator, separatorRowHeight),
							(SettingsRow.PreferredCharts, settingsRowHeight),
							(SettingsRow.PreferredChartsSeparator, separatorRowHeight),
							(SettingsRow.CopyrightPadding, 20),
							(SettingsRow.Copyright, Star)),

						ColumnDefinitions = Columns.Define(
							(SettingsColumn.Icon, 24),
							(SettingsColumn.Title, Star),
							(SettingsColumn.Button, 100)),

						Children =
						{
							new GitHubUserView()
								.Row(SettingsRow.GitHubUser).ColumnSpan(All<SettingsColumn>()),

							new Separator()
								.Row(SettingsRow.GitHubUserSeparator).ColumnSpan(All<SettingsColumn>()),

							new AboutRowTappableView(aboutRowTapGesture)
								.Row(SettingsRow.About).ColumnSpan(All<SettingsColumn>()),
							new AboutRowSvg(deviceInfo, "about.svg")
								.Row(SettingsRow.About).Column(SettingsColumn.Icon),
							new AboutTitleLabel(SettingsPageAutomationIds.AboutTitleLabel)
								.Row(SettingsRow.About).Column(SettingsColumn.Title)
								.Bind(Label.TextProperty,
									getter: static (SettingsViewModel vm) => vm.AboutLabelText,
									mode: BindingMode.OneTime),
							new AboutRowSvg(deviceInfo, "right_arrow.svg").End()
								.Row(SettingsRow.About).Column(SettingsColumn.Button),

							new Separator()
								.Row(SettingsRow.AboutSeparator).ColumnSpan(All<SettingsColumn>()),

							new LoginRowTappableView(loginRowTapGesture)
								.Row(SettingsRow.Login).ColumnSpan(All<SettingsColumn>()),
							new LoginRowSvg(deviceInfo, "logout.svg")
								.Row(SettingsRow.Login).Column(SettingsColumn.Icon),
							new LoginLabel()
								.Row(SettingsRow.Login).Column(SettingsColumn.Title),
							new LoginRowSvg(deviceInfo, "right_arrow.svg").End()
								.Row(SettingsRow.Login).Column(SettingsColumn.Button),

							new Separator()
								.Row(SettingsRow.LoginSeparator).ColumnSpan(All<SettingsColumn>()),

							new SvgImage(deviceInfo, "organization.svg", () => AppResources.GetResource<Color>(nameof(BaseTheme.IconColor)))
								.Row(SettingsRow.Organizations).Column(SettingsColumn.Icon),
							new SettingsTitleLabel(SettingsPageAutomationIds.IncludeOrganizationsSwitch)
								.Row(SettingsRow.Organizations).Column(SettingsColumn.Title)
								.Bind(Label.TextProperty, 
									getter: static (SettingsViewModel vm) => vm.ShouldIncludeOrganizationsLabelText,
									mode: BindingMode.OneTime),
							new IncludeOrganizationsSwitch(deviceInfo)
								.Row(SettingsRow.Organizations).Column(SettingsColumn.Button),
							new Separator()
								.Row(SettingsRow.OrganizationsSeparator).ColumnSpan(All<SettingsColumn>()),
							new SvgImage(deviceInfo, "bell.svg", () => AppResources.GetResource<Color>(nameof(BaseTheme.IconColor)))
								.Row(SettingsRow.Notifications).Column(SettingsColumn.Icon),
							new SettingsTitleLabel(SettingsPageAutomationIds.RegisterForNotificationsTitleLabel)
								.Row(SettingsRow.Notifications).Column(SettingsColumn.Title)
								.Bind(Label.TextProperty, 
									getter: static (SettingsViewModel vm) => vm.RegisterForNotificationsLabelText,
									mode: BindingMode.OneTime),
							new EnableNotificationsSwitch(deviceInfo)
								.Row(SettingsRow.Notifications).Column(SettingsColumn.Button),
							new Separator()
								.Row(SettingsRow.NotificationsSeparator).ColumnSpan(All<SettingsColumn>()),
							new SvgImage(deviceInfo, "theme.svg", () => AppResources.GetResource<Color>(nameof(BaseTheme.IconColor)))
								.Row(SettingsRow.Theme).Column(SettingsColumn.Icon),
							new SettingsTitleLabel(SettingsPageAutomationIds.ThemeTitleLabel)
								.Row(SettingsRow.Theme).Column(SettingsColumn.Title)
								.Bind(Label.TextProperty, 
									getter: static (SettingsViewModel vm) => vm.ThemeLabelText,
									mode: BindingMode.OneTime),
							new SettingsPicker(SettingsPageAutomationIds.ThemePicker, 70)
								.Row(SettingsRow.Theme).Column(SettingsColumn.Button)
								.Bind(Picker.ItemsSourceProperty, 
									getter: static (SettingsViewModel vm) => vm.ThemePickerItemsSource,
									mode: BindingMode.OneTime)
								.Bind(Picker.SelectedIndexProperty, 
									getter: static (SettingsViewModel vm) => vm.ThemePickerSelectedIndex,
									setter: static (vm, index) => vm.ThemePickerSelectedIndex = index),

							new Separator()
								.Row(SettingsRow.ThemeSeparator).ColumnSpan(All<SettingsColumn>()),

							new SvgImage(deviceInfo, "language.svg", () => AppResources.GetResource<Color>(nameof(BaseTheme.IconColor)))
								.Row(SettingsRow.Language).Column(SettingsColumn.Icon),
							new SettingsTitleLabel(SettingsPageAutomationIds.LanguageTitleLabel)
								.Row(SettingsRow.Language).Column(SettingsColumn.Title)
								.Bind(Label.TextProperty, nameof(SettingsViewModel.LanguageLabelText)),
							new SettingsPicker(SettingsPageAutomationIds.LanguagePicker, 100)
								.Row(SettingsRow.Language).Column(SettingsColumn.Button)
								.Bind(Picker.ItemsSourceProperty, 
									getter: static (SettingsViewModel vm) => vm.LanguagePickerItemsSource,
									mode: BindingMode.OneTime)
								.Bind(Picker.SelectedIndexProperty, 
									getter: static (SettingsViewModel vm) => vm.LanguagePickerSelectedIndex,
									setter: static (vm, index) => vm.LanguagePickerSelectedIndex = index),
							
							new Separator()
								.Row(SettingsRow.LanguageSeparator).ColumnSpan(All<SettingsColumn>()),
							
							new SvgImage(deviceInfo, "chart.svg", () => AppResources.GetResource<Color>(nameof(BaseTheme.IconColor)))
								.Row(SettingsRow.PreferredCharts).Column(SettingsColumn.Icon),
							new SettingsTitleLabel(SettingsPageAutomationIds.PreferredChartTitleLabel)
								.Row(SettingsRow.PreferredCharts).Column(SettingsColumn.Title)
								.Bind(Label.TextProperty, 
									getter: static (SettingsViewModel vm) => vm.PreferredChartsLabelText),
							new SettingsPicker(SettingsPageAutomationIds.PreferredChartsPicker, 100)
								.Row(SettingsRow.PreferredCharts).Column(SettingsColumn.Button)
								.Bind(Picker.ItemsSourceProperty, 
									getter: static (SettingsViewModel vm) => vm.PreferredChartsItemsSource,
									mode: BindingMode.OneTime)
								.Bind(Picker.SelectedIndexProperty, 
									getter: static (SettingsViewModel vm) => vm.PreferredChartsSelectedIndex,
									setter: static (vm, index) => vm.PreferredChartsSelectedIndex = index),
							new Separator()
								.Row(SettingsRow.PreferredChartsSeparator).ColumnSpan(All<SettingsColumn>()),
							new CopyrightLabel()
								.Row(SettingsRow.Copyright).ColumnSpan(All<SettingsColumn>()),
						}
					}.Assign(out _contentGrid)
				}.Paddings(left: 28, right: 28, bottom: 8),

				new OrganizationsCarouselOverlay(deviceInfo, analyticsService).Assign(out _organizationsCarouselOverlay),
			}
		};

		this.SetBinding(TitleProperty, nameof(SettingsViewModel.TitleText));
	}

	enum SettingsRow { GitHubUser, GitHubUserSeparator, About, AboutSeparator, Login, LoginSeparator, Organizations, OrganizationsSeparator, Notifications, NotificationsSeparator, Theme, ThemeSeparator, Language, LanguageSeparator, PreferredCharts, PreferredChartsSeparator, CopyrightPadding, Copyright }
	enum SettingsColumn { Icon, Title, Button }

	public void Dispose()
	{
		_connectToGitHubCancellationTokenSource.Dispose();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		_connectToGitHubCancellationTokenSource = new();
	}

	protected override void OnDisappearing()
	{
		_connectToGitHubCancellationTokenSource.Cancel();

		base.OnDisappearing();
	}

	async void HandleAboutRowTapped(object? sender, EventArgs e)
	{
		AnalyticsService.Track("About Button Tapped");

		var aboutPage = IPlatformApplication.Current?.Services.GetRequiredService<AboutPage>();
		await Navigation.PushAsync(aboutPage);
	}

	async void HandleLoginRowTapped(object? sender, EventArgs e)
	{
		AnalyticsService.Track("Login Button Tapped", nameof(SettingsViewModel.IsNotAuthenticating), BindingContext.IsNotAuthenticating.ToString());

		if (BindingContext.IsNotAuthenticating)
		{
			var loginRowViews = _contentGrid.Children.OfType<ILoginRowView>().Cast<View>();

			await Task.WhenAll(loginRowViews.Select(static x => x.FadeTo(0.3, 75)));

			BindingContext.HandleConnectToGitHubButtonCommand.Execute((_connectToGitHubCancellationTokenSource.Token, null));

			await Task.WhenAll(loginRowViews.Select(static x => x.FadeTo(1, 350, Easing.CubicOut)));
		}
	}

	async void HandleOrganizationsCarouselViewVisibilityChanged(object? sender, bool isVisible) => await Dispatcher.DispatchAsync(async () =>
	{
		if (isVisible)
			await _organizationsCarouselOverlay.Reveal(true);
		else
			await _organizationsCarouselOverlay.Dismiss(true);
	});

	sealed class AboutRowTappableView : ContentView
	{
		public AboutRowTappableView(TapGestureRecognizer tapGestureRecognizer) => GestureRecognizers.Add(tapGestureRecognizer);
	}

	sealed class LoginRowTappableView : ContentView, ILoginRowView
	{
		public LoginRowTappableView(TapGestureRecognizer tapGestureRecognizer) => GestureRecognizers.Add(tapGestureRecognizer);
	}

	sealed class LoginRowSvg : SvgImage, ILoginRowView
	{
		public LoginRowSvg(in IDeviceInfo deviceInfo, in string svgFileName) : base(deviceInfo, svgFileName, () => AppResources.GetResource<Color>(nameof(BaseTheme.IconColor)))
		{
			//Allow LoginRowTappableView to handle taps
			InputTransparent = true;
		}
	}

	sealed class LoginLabel : TitleLabel, ILoginRowView
	{
		public LoginLabel()
		{
			this.Fill();
			HorizontalTextAlignment = TextAlignment.Start;

			AutomationId = SettingsPageAutomationIds.LoginTitleLabel;

			//Allow LoginRowTappableView to handle taps
			InputTransparent = true;

			this.SetBinding(TextProperty, nameof(SettingsViewModel.LoginLabelText));
		}
	}

	sealed class AboutTitleLabel : SettingsTitleLabel
	{
		public AboutTitleLabel(in string automationId) : base(automationId)
		{
			//Allow AboutRowTappableView to handle taps
			InputTransparent = true;
		}
	}

	sealed class AboutRowSvg : SvgImage
	{
		public AboutRowSvg(in IDeviceInfo deviceInfo, in string svgFileName) : base(deviceInfo, svgFileName, () => AppResources.GetResource<Color>(nameof(BaseTheme.IconColor)))
		{
			//Allow AboutRowTappableView to handle taps
			InputTransparent = true;
			AutomationId = SettingsPageAutomationIds.AboutButton;
		}
	}

	class SettingsTitleLabel : TitleLabel
	{
		public SettingsTitleLabel(in string automationId) => AutomationId = automationId;
	}

	sealed class Separator : BoxView
	{
		public Separator() => this.DynamicResource(ColorProperty, nameof(BaseTheme.SeparatorColor));
	}

	sealed class EnableNotificationsSwitch : SettingsSwitch
	{
		public EnableNotificationsSwitch(in IDeviceInfo deviceInfo) : base(deviceInfo)
		{
			AutomationId = SettingsPageAutomationIds.RegisterForNotificationsSwitch;

			this.SetBinding(IsToggledProperty, nameof(SettingsViewModel.IsRegisterForNotificationsSwitchToggled));
			this.SetBinding(IsEnabledProperty, nameof(SettingsViewModel.IsRegisterForNotificationsSwitchEnabled));
		}
	}

	sealed class IncludeOrganizationsSwitch : SettingsSwitch
	{
		public IncludeOrganizationsSwitch(in IDeviceInfo deviceInfo) : base(deviceInfo)
		{
			AutomationId = SettingsPageAutomationIds.IncludeOrganizationsSwitch;

			this.SetBinding(IsToggledProperty, nameof(SettingsViewModel.IsShouldIncludeOrganizationsSwitchToggled));
			this.SetBinding(IsEnabledProperty, nameof(SettingsViewModel.IsShouldIncludeOrganizationsSwitchEnabled));
		}
	}

	sealed class SettingsPicker : Picker
	{
		public SettingsPicker(in string automationId, in int widthRequest)
		{
			FontSize = 12;
			WidthRequest = widthRequest;
			AutomationId = automationId;
			FontFamily = FontFamilyConstants.RobotoMedium;

			this.End();

			this.DynamicResources((TextColorProperty, nameof(BaseTheme.SettingsLabelTextColor)),
				(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor)));
		}
	}

	interface ILoginRowView;
}