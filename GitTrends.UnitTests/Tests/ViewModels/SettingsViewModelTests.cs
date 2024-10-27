using GitTrends.Common;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using Shiny;

namespace GitTrends.UnitTests;

class SettingsViewModelTests : BaseTest
{
	[Test]
	public async Task RegisterForNotificationsTest()
	{
		//Arrange
		bool shouldSendNotifications_Initial, shouldSendNotifications_Final;
		bool isRegisterForNotificationsSwitchEnabled_Initial, isRegisterForNotificationsSwitchEnabled_Final;
		bool isRegisterForNotificationsSwitchToggled_Initial, isRegisterForNotificationsSwitchToggled_Final;

		bool didSetNotificationsPreferenceCompletedFire = false;
		var setNotificationsPreferenceCompletedTCS = new TaskCompletionSource<AccessState?>();

		var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();
		SettingsViewModel.SetNotificationsPreferenceCompleted += HandleSetNotificationsPreferenceCompleted;

		var notificationService = ServiceCollection.ServiceProvider.GetRequiredService<NotificationService>();

		//Act
		shouldSendNotifications_Initial = notificationService.ShouldSendNotifications;

		isRegisterForNotificationsSwitchEnabled_Initial = settingsViewModel.IsRegisterForNotificationsSwitchEnabled;
		isRegisterForNotificationsSwitchToggled_Initial = settingsViewModel.IsRegisterForNotificationsSwitchToggled;

		settingsViewModel.IsRegisterForNotificationsSwitchToggled = true;
		var accessState = await setNotificationsPreferenceCompletedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		shouldSendNotifications_Final = notificationService.ShouldSendNotifications;

		isRegisterForNotificationsSwitchEnabled_Final = settingsViewModel.IsRegisterForNotificationsSwitchEnabled;
		isRegisterForNotificationsSwitchToggled_Final = settingsViewModel.IsRegisterForNotificationsSwitchToggled;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didSetNotificationsPreferenceCompletedFire);

			Assert.That(shouldSendNotifications_Initial, Is.False);
			Assert.That(isRegisterForNotificationsSwitchEnabled_Initial);
			Assert.That(isRegisterForNotificationsSwitchToggled_Initial, Is.False);

			Assert.That(shouldSendNotifications_Final);
			Assert.That(isRegisterForNotificationsSwitchEnabled_Final);
			Assert.That(isRegisterForNotificationsSwitchToggled_Final);

			Assert.That(accessState, Is.EqualTo(AccessState.Available));
		});

		void HandleSetNotificationsPreferenceCompleted(object? sender, AccessState? e)
		{
			SettingsViewModel.SetNotificationsPreferenceCompleted -= HandleSetNotificationsPreferenceCompleted;

			didSetNotificationsPreferenceCompletedFire = true;
			setNotificationsPreferenceCompletedTCS.SetResult(e);
		}
	}

	[Test]
	public void PreferredChartsTest()
	{
		//Arrange
		int preferredChartsIndex_Initial, preferredChartsIndex_AfterJustUniques, preferredChartsIndex_AfterNoUniques, preferredChartsIndex_AfterAll;
		TrendsChartOption currentTrendsChartOption_Initial, currentTrendsChartOption_AfterJustUniques, currentTrendsChartOption_AfterNoUniques, currentTrendsChartOption_AfterAll;

		var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();
		var trendsChartSettingsService = ServiceCollection.ServiceProvider.GetRequiredService<TrendsChartSettingsService>();

		//Act
		currentTrendsChartOption_Initial = trendsChartSettingsService.CurrentTrendsChartOption;
		preferredChartsIndex_Initial = settingsViewModel.PreferredChartsSelectedIndex;

		settingsViewModel.PreferredChartsSelectedIndex = (int)TrendsChartOption.JustUniques;

		currentTrendsChartOption_AfterJustUniques = trendsChartSettingsService.CurrentTrendsChartOption;
		preferredChartsIndex_AfterJustUniques = settingsViewModel.PreferredChartsSelectedIndex;

		settingsViewModel.PreferredChartsSelectedIndex = (int)TrendsChartOption.NoUniques;

		currentTrendsChartOption_AfterNoUniques = trendsChartSettingsService.CurrentTrendsChartOption;
		preferredChartsIndex_AfterNoUniques = settingsViewModel.PreferredChartsSelectedIndex;

		settingsViewModel.PreferredChartsSelectedIndex = (int)TrendsChartOption.All;

		currentTrendsChartOption_AfterAll = trendsChartSettingsService.CurrentTrendsChartOption;
		preferredChartsIndex_AfterAll = settingsViewModel.PreferredChartsSelectedIndex;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(currentTrendsChartOption_Initial, Is.EqualTo(TrendsChartOption.All));
			Assert.That((TrendsChartOption)preferredChartsIndex_Initial, Is.EqualTo(TrendsChartOption.All));

			Assert.That(currentTrendsChartOption_AfterJustUniques, Is.EqualTo(TrendsChartOption.JustUniques));
			Assert.That((TrendsChartOption)preferredChartsIndex_AfterJustUniques, Is.EqualTo(TrendsChartOption.JustUniques));

			Assert.That(currentTrendsChartOption_AfterNoUniques, Is.EqualTo(TrendsChartOption.NoUniques));
			Assert.That((TrendsChartOption)preferredChartsIndex_AfterNoUniques, Is.EqualTo(TrendsChartOption.NoUniques));

			Assert.That(currentTrendsChartOption_AfterAll, Is.EqualTo(TrendsChartOption.All));
			Assert.That((TrendsChartOption)preferredChartsIndex_AfterAll, Is.EqualTo(TrendsChartOption.All));
		});
	}

	[Test]
	public async Task DemoButtonCommandTest()
	{
		//Arrange
		bool isDemoButtonVisible_Initial, isDemoButtonVisible_Final;

		string loginLabelText_Initial, loginLabelText_Final;
		string gitHubNameLabelText_Initial, gitHubNameLabelText_Final;
		string gitHubAliasLabelText_Initial, gitHubAliasLabelText_Final;
		string gitHubAvatarImageSource_Initial, gitHubAvatarImageSource_Final;

		var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();
		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();

		//Act
		loginLabelText_Initial = settingsViewModel.LoginLabelText;
		isDemoButtonVisible_Initial = settingsViewModel.IsDemoButtonVisible;
		gitHubNameLabelText_Initial = settingsViewModel.GitHubNameLabelText;
		gitHubAliasLabelText_Initial = settingsViewModel.GitHubAliasLabelText;
		gitHubAvatarImageSource_Initial = settingsViewModel.GitHubAvatarImageSource;

		await settingsViewModel.HandleDemoButtonTappedCommand.ExecuteAsync(null).ConfigureAwait(false);

		loginLabelText_Final = settingsViewModel.LoginLabelText;
		isDemoButtonVisible_Final = settingsViewModel.IsDemoButtonVisible;
		gitHubNameLabelText_Final = settingsViewModel.GitHubNameLabelText;
		gitHubAliasLabelText_Final = settingsViewModel.GitHubAliasLabelText;
		gitHubAvatarImageSource_Final = settingsViewModel.GitHubAvatarImageSource;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(gitHubUserService.IsDemoUser);

			Assert.That(isDemoButtonVisible_Initial);
			Assert.That(isDemoButtonVisible_Final, Is.False);

			Assert.That(gitHubAliasLabelText_Initial, Is.EqualTo(string.Empty));
			Assert.That($"@{DemoUserConstants.Alias}", Is.EqualTo(gitHubAliasLabelText_Final));
			Assert.That(gitHubAliasLabelText_Final, Is.EqualTo("@" + gitHubUserService.Alias));

			Assert.That(GitHubLoginButtonConstants.NotLoggedIn, Is.EqualTo(gitHubNameLabelText_Initial));
			Assert.That(DemoUserConstants.Name, Is.EqualTo(gitHubNameLabelText_Final));
			Assert.That(gitHubNameLabelText_Final, Is.EqualTo(gitHubUserService.Name));

			Assert.That(gitHubAvatarImageSource_Initial, Is.EqualTo("DefaultProfileImage"));
			Assert.That(gitHubAvatarImageSource_Final, Is.EqualTo("GitTrends"));
			Assert.That(gitHubAvatarImageSource_Final, Is.EqualTo(BaseTheme.GetGitTrendsImageSource()));

			Assert.That(loginLabelText_Initial, Is.EqualTo(GitHubLoginButtonConstants.ConnectToGitHub));
			Assert.That(loginLabelText_Final, Is.EqualTo(GitHubLoginButtonConstants.Disconnect));
		});
	}

	[Test]
	public async Task ConnectToGitHubButtonCommandTest()
	{
		//Arrange
		string openedUrl;
		bool isAuthenticating_BeforeCommand, isAuthenticating_DuringCommand, isAuthenticating_AfterCommand;
		bool isNotAuthenticating_BeforeCommand, isNotAuthenticating_DuringCommand, isNotAuthenticating_AfterCommand;
		bool isDemoButtonVisible_BeforeCommand, isDemoButtonVisible_DuringCommand, isDemoButtonVisible_AfterCommand;

		bool didOpenAsyncFire = false;
		var openAsyncExecutedTCS = new TaskCompletionSource<Uri>();

		MockBrowser.OpenAsyncExecuted += HandleOpenAsyncExecuted;

		var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();
		var gitTrendsStatisticsService = ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsStatisticsService>();

		//Act
		await gitTrendsStatisticsService.Initialize(CancellationToken.None).ConfigureAwait(false);

		isNotAuthenticating_BeforeCommand = settingsViewModel.IsNotAuthenticating;
		isAuthenticating_BeforeCommand = settingsViewModel.IsAuthenticating;
		isDemoButtonVisible_BeforeCommand = settingsViewModel.IsDemoButtonVisible;

		var connectToGitHubButtonCommandTask = settingsViewModel.HandleConnectToGitHubButtonCommand.ExecuteAsync((CancellationToken.None, null));
		isNotAuthenticating_DuringCommand = settingsViewModel.IsNotAuthenticating;
		isAuthenticating_DuringCommand = settingsViewModel.IsAuthenticating;
		isDemoButtonVisible_DuringCommand = settingsViewModel.IsDemoButtonVisible;

		await connectToGitHubButtonCommandTask.ConfigureAwait(false);
		var openedUri = await openAsyncExecutedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);
		openedUrl = openedUri.AbsoluteUri;

		isNotAuthenticating_AfterCommand = settingsViewModel.IsNotAuthenticating;
		isAuthenticating_AfterCommand = settingsViewModel.IsAuthenticating;
		isDemoButtonVisible_AfterCommand = settingsViewModel.IsDemoButtonVisible;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(didOpenAsyncFire);

			Assert.That(isAuthenticating_BeforeCommand, Is.False);
			Assert.That(isNotAuthenticating_BeforeCommand);
			Assert.That(isDemoButtonVisible_BeforeCommand);

			Assert.That(isAuthenticating_DuringCommand);
			Assert.That(isNotAuthenticating_DuringCommand, Is.False);
			Assert.That(isDemoButtonVisible_DuringCommand, Is.False);

			Assert.That(isAuthenticating_AfterCommand, Is.False);
			Assert.That(isNotAuthenticating_AfterCommand);
			Assert.That(isDemoButtonVisible_AfterCommand);

			Assert.That(openedUrl, Does.Contain($"{GitHubConstants.GitHubBaseUrl}/login/oauth/authorize?client_id="));
			Assert.That(openedUrl, Does.Contain($"&scope={GitHubConstants.OAuthScope}&state="));
		});

		void HandleOpenAsyncExecuted(object? sender, Uri e)
		{
			MockBrowser.OpenAsyncExecuted -= HandleOpenAsyncExecuted;
			didOpenAsyncFire = true;

			openAsyncExecutedTCS.SetResult(e);
		}
	}

	[Test]
	public void ThemePickerItemsSourceTest()
	{
		//Arrange
		int themePickerIndex_Initial, themePickerIndex_AfterDarkTheme, themePickerIndex_AfterLightTheme, themePickerIndex_AfterDefaultTheme;
		IReadOnlyList<string> themePickerItemSource_Initial, themePickerItemSource_Final;

		var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();

		//Act
		themePickerItemSource_Initial = settingsViewModel.ThemePickerItemsSource;
		themePickerIndex_Initial = settingsViewModel.ThemePickerSelectedIndex;

		settingsViewModel.ThemePickerSelectedIndex = (int)PreferredTheme.Dark;
		themePickerIndex_AfterDarkTheme = settingsViewModel.ThemePickerSelectedIndex;

		settingsViewModel.ThemePickerSelectedIndex = (int)PreferredTheme.Light;
		themePickerIndex_AfterLightTheme = settingsViewModel.ThemePickerSelectedIndex;

		settingsViewModel.ThemePickerSelectedIndex = (int)PreferredTheme.Default;
		themePickerIndex_AfterDefaultTheme = settingsViewModel.ThemePickerSelectedIndex;
		themePickerItemSource_Final = settingsViewModel.ThemePickerItemsSource;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(Enum.GetNames(typeof(PreferredTheme)), Is.EqualTo(themePickerItemSource_Initial));
			Assert.That(themePickerItemSource_Initial, Is.EqualTo(themePickerItemSource_Final));

			Assert.That((PreferredTheme)themePickerIndex_AfterDarkTheme, Is.EqualTo(PreferredTheme.Dark));
			Assert.That((PreferredTheme)themePickerIndex_AfterLightTheme, Is.EqualTo(PreferredTheme.Light));
			Assert.That((PreferredTheme)themePickerIndex_AfterDefaultTheme, Is.EqualTo(PreferredTheme.Default));
		});
	}


	[Test]
	public async Task CopyrightLabelTappedCommandTest()
	{
		//Arrange
		bool didOpenAsyncFire = false;
		var openAsyncTCS = new TaskCompletionSource();

		var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();

		MockLauncher.OpenAsyncExecuted += HandleOpenAsyncExecuted;

		//Act
		await settingsViewModel.CopyrightLabelTappedCommand.ExecuteAsync(null).ConfigureAwait(false);
		await openAsyncTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		//Assert
		Assert.That(didOpenAsyncFire);

		void HandleOpenAsyncExecuted(object? sender, EventArgs e)
		{
			MockLauncher.OpenAsyncExecuted -= HandleOpenAsyncExecuted;

			didOpenAsyncFire = true;
			openAsyncTCS.SetResult();
		}
	}

	[Test]
	public async Task ShouldIncludeOrganizationsSwitchTest()
	{
		//Arrange
		bool isShouldIncludeOrganizationsSwitchEnabled_Initial, isShouldIncludeOrganizationsSwitchEnabled_Final;
		bool isShouldIncludeOrganizationsSwitchToggled_Initial, isShouldIncludeOrganizationsSwitchToggled_Final;

		var organizationsCarouselViewVisibilityChangedTCS = new TaskCompletionSource<bool>();

		var gitHubUserService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubUserService>();
		var settingsViewModel = ServiceCollection.ServiceProvider.GetRequiredService<SettingsViewModel>();
		var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();
		var azureFunctionsApiService = ServiceCollection.ServiceProvider.GetRequiredService<AzureFunctionsApiService>();
		var gitTrendsStatisticsService = ServiceCollection.ServiceProvider.GetRequiredService<GitTrendsStatisticsService>();

		SettingsViewModel.OrganizationsCarouselViewVisibilityChanged += HandleOrganizationsCarouselViewVisibilityChanged;

		//Act
		await gitTrendsStatisticsService.Initialize(CancellationToken.None).ConfigureAwait(false);

		var clientIdDTO = await azureFunctionsApiService.GetGitHubClientId(CancellationToken.None).ConfigureAwait(false);
		var clientId = clientIdDTO.ClientId;

		isShouldIncludeOrganizationsSwitchEnabled_Initial = settingsViewModel.IsShouldIncludeOrganizationsSwitchEnabled;
		isShouldIncludeOrganizationsSwitchToggled_Initial = settingsViewModel.IsShouldIncludeOrganizationsSwitchToggled;

		await AuthenticateUser(gitHubUserService, gitHubGraphQLApiService, TestCancellationTokenSource.Token).ConfigureAwait(false);

		isShouldIncludeOrganizationsSwitchEnabled_Final = settingsViewModel.IsShouldIncludeOrganizationsSwitchEnabled;

		settingsViewModel.IsShouldIncludeOrganizationsSwitchToggled = true;
		var organizationsCarouselViewVisiblilityChangedResult = await organizationsCarouselViewVisibilityChangedTCS.Task.WaitAsync(TestCancellationTokenSource.Token).ConfigureAwait(false);

		isShouldIncludeOrganizationsSwitchToggled_Final = settingsViewModel.IsShouldIncludeOrganizationsSwitchToggled;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(isShouldIncludeOrganizationsSwitchEnabled_Initial, Is.False);
			Assert.That(isShouldIncludeOrganizationsSwitchToggled_Initial, Is.False);

			Assert.That(isShouldIncludeOrganizationsSwitchEnabled_Final);
			Assert.That(isShouldIncludeOrganizationsSwitchToggled_Final);

			Assert.That(organizationsCarouselViewVisiblilityChangedResult);
		});

		void HandleOrganizationsCarouselViewVisibilityChanged(object? sender, bool e)
		{
			SettingsViewModel.OrganizationsCarouselViewVisibilityChanged -= HandleOrganizationsCarouselViewVisibilityChanged;
			organizationsCarouselViewVisibilityChangedTCS.SetResult(e);
		}
	}
}