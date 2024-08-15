using System.ComponentModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.Input;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends;

public partial class RepositoryPage : BaseContentPage<RepositoryViewModel>, ISearchPage
{
	readonly WeakEventManager<string> _searchTextChangedEventManager = new();

	readonly RefreshView _refreshView;
	readonly FirstRunService _firstRunService;
	readonly GitHubUserService _gitHubUserService;
	readonly DeepLinkingService _deepLinkingService;

	public RepositoryPage(IDeviceInfo deviceInfo,
		FirstRunService firstRunService,
		IAnalyticsService analyticsService,
		GitHubUserService gitHubUserService,
		DeepLinkingService deepLinkingService,
		RepositoryViewModel repositoryViewModel,
		MobileSortingService mobileSortingService) : base(repositoryViewModel, analyticsService)
	{
		Shell.SetPresentationMode(this, PresentationMode.NotAnimated);
		Shell.SetBackButtonBehavior(this, new BackButtonBehavior
		{
			IsVisible = false,
			IsEnabled = false
		});

		_firstRunService = firstRunService;
		_gitHubUserService = gitHubUserService;
		_deepLinkingService = deepLinkingService;

		SearchBarTextChanged += HandleSearchBarTextChanged;
		RepositoryViewModel.PullToRefreshFailed += HandlePullToRefreshFailed;
		LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

		this.SetBinding(TitleProperty, nameof(RepositoryViewModel.TitleText));

		ToolbarItems.Add(new ToolbarItem
		{
			Text = PageTitles.SettingsPage,
			IconImageSource = deviceInfo.Platform == DevicePlatform.iOS ? "Settings" : null,
			Order = deviceInfo.Platform == DevicePlatform.Android ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Default,
			AutomationId = RepositoryPageAutomationIds.SettingsButton,
			Command = new AsyncRelayCommand(ExecuteSettingsToolbarItemCommand)
		});

		ToolbarItems.Add(new ToolbarItem
		{
			Text = RepositoryPageConstants.SortToolbarItemText,
			Priority = 1,
			IconImageSource = deviceInfo.Platform == DevicePlatform.iOS ? "Sort" : null,
			Order = deviceInfo.Platform == DevicePlatform.Android ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Default,
			AutomationId = RepositoryPageAutomationIds.SortButton,
			Command = new AsyncRelayCommand(ExecuteSortToolbarItemCommand)
		});

		Content = new Grid
		{
			RowDefinitions = Rows.Define(
				(Row.CollectionView, Star),
				(Row.Information, InformationButton.Diameter)),

			ColumnDefinitions = Columns.Define(
				(Column.CollectionView, Star),
				(Column.Information, InformationButton.Diameter)),

			Children =
			{
				//Work around to prevent iOS Navigation Bar from collapsing
				new BoxView
					{
						HeightRequest = 0.1
					}
					.RowSpan(All<Row>()).ColumnSpan(All<Column>()),

				new RefreshView
					{
						AutomationId = RepositoryPageAutomationIds.RefreshView,
						Content = new CollectionView
						{
							ItemTemplate = new RepositoryDataTemplateSelector(deviceInfo, mobileSortingService),
							BackgroundColor = Colors.Transparent,
							AutomationId = RepositoryPageAutomationIds.CollectionView,
							
							//Work around for https://github.com/xamarin/Xamarin.Forms/issues/9879
							Header = deviceInfo.Platform == DevicePlatform.Android ? new BoxView
							{
								HeightRequest = BaseRepositoryDataTemplate.BottomPadding
							} : null,
							Footer = deviceInfo.Platform == DevicePlatform.Android ? new BoxView
							{
								HeightRequest = BaseRepositoryDataTemplate.TopPadding
							} : null,
							EmptyView = new EmptyDataView("EmptyRepositoriesList", RepositoryPageAutomationIds.EmptyDataView)
								.Bind<EmptyDataView, bool, bool>(IsVisibleProperty, nameof(RepositoryViewModel.IsRefreshing), convert: static isRefreshing => !isRefreshing)
								.Bind(EmptyDataView.TitleProperty, nameof(RepositoryViewModel.EmptyDataViewTitle))
								.Bind(EmptyDataView.DescriptionProperty, nameof(RepositoryViewModel.EmptyDataViewDescription))

						}.Bind(CollectionView.ItemsSourceProperty, nameof(RepositoryViewModel.VisibleRepositoryList))

					}.RowSpan(All<Row>()).ColumnSpan(All<Column>()).Assign(out _refreshView)
					.Bind(RefreshView.IsRefreshingProperty, nameof(RepositoryViewModel.IsRefreshing))
					.Bind(RefreshView.CommandProperty, nameof(RepositoryViewModel.ExecuteRefreshCommand))
					.DynamicResource(RefreshView.RefreshColorProperty, nameof(BaseTheme.PullToRefreshColor)),

				new InformationButton(mobileSortingService, analyticsService).Row(Row.Information).Column(Column.Information)
			}
		};
	}

	enum Row { CollectionView, Information }
	enum Column { CollectionView, Information }

	public event EventHandler<string> SearchBarTextChanged
	{
		add => _searchTextChangedEventManager.AddEventHandler(value);
		remove => _searchTextChangedEventManager.RemoveEventHandler(value);
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);

		var token = await _gitHubUserService.GetGitHubToken();

		switch (_firstRunService.IsFirstRun)
		{
			case true when !Shell.Current.Navigation.ModalStack.OfType<OnboardingPage>().Any():
				await NavigateToOnboardingPage();
				break;

			case false when shouldShowWelcomePage(Shell.Current.Navigation, token.AccessToken):
				await NavigateToWelcomePage();
				break;

			case false when isUserValid(token.AccessToken, _gitHubUserService.Alias)
				&& _refreshView.Content is CollectionView collectionView
				&& collectionView.ItemsSource.IsNullOrEmpty():
				_refreshView.IsRefreshing = true;
				break;
		}

		bool shouldShowWelcomePage(in INavigation navigation, in string accessToken)
		{
			return !navigation.ModalStack.Any()
				&& _gitHubUserService.Alias != DemoUserConstants.Alias
				&& !isUserValid(accessToken, _gitHubUserService.Alias);
		}

		static bool isUserValid(in string accessToken, in string alias) => !string.IsNullOrWhiteSpace(accessToken) || !string.IsNullOrWhiteSpace(alias);
	}

	[RelayCommand]
	Task RepositoryDataTemplateTapped(Repository repository)
	{
		AnalyticsService.Track("Repository Tapped", new Dictionary<string, string>
		{
			{
				nameof(Repository) + nameof(Repository.OwnerLogin), repository.OwnerLogin
			},
			{
				nameof(Repository) + nameof(Repository.Name), repository.Name
			}
		});

		return NavigateToTrendsPage(repository);
	}

	async Task NavigateToWelcomePage()
	{
		//Allow RepositoryPage to appear briefly before loading 
		await Task.Delay(TimeSpan.FromMilliseconds(250));
		await Dispatcher.DispatchAsync(() => Shell.Current.GoToAsync(AppShell.GetPageRoute<WelcomePage>()));
	}

	async Task NavigateToOnboardingPage()
	{
		//Allow RepositoryPage to appear briefly before loading 
		await Task.Delay(TimeSpan.FromMilliseconds(500));
		await Dispatcher.DispatchAsync(() => Shell.Current.GoToAsync(AppShell.GetPageRoute<OnboardingPage>()));
	}

	Task NavigateToSettingsPage() =>
		Dispatcher.DispatchAsync(() => Shell.Current.GoToAsync(AppShell.GetPageRoute<SettingsPage>()));

	Task NavigateToTrendsPage(in Repository repository)
	{
		var parameters = new Dictionary<string, object>
		{
			{
				TrendsViewModel.RepositoryQueryString, repository
			}
		};

		return Dispatcher.DispatchAsync(() => Shell.Current.GoToAsync(AppShell.GetPageRoute<TrendsPage>(), parameters));
	}

	Task ExecuteSettingsToolbarItemCommand()
	{
		AnalyticsService.Track("Settings Button Tapped");

		return NavigateToSettingsPage();
	}

	async Task ExecuteSortToolbarItemCommand()
	{
		var sortingOptions = MobileSortingService.SortingOptionsDictionary.Values;

		string? selection = await DisplayActionSheet(SortingConstants.ActionSheetTitle, SortingConstants.CancelText, null, sortingOptions.ToArray());

		if (!string.IsNullOrWhiteSpace(selection) && selection != SortingConstants.CancelText)
			BindingContext.SortRepositoriesCommand.Execute(MobileSortingService.SortingOptionsDictionary.First(x => x.Value == selection).Key);
	}

	async void HandlePullToRefreshFailed(object? sender, PullToRefreshFailedEventArgs eventArgs) => await Dispatcher.DispatchAsync(async () =>
	{
		if (Application.Current is App
			&& !Application.Current.MainPage?.Navigation.ModalStack.Any() is true
			&& Application.Current.MainPage?.Navigation.NavigationStack.Last() is RepositoryPage)
		{
			switch (eventArgs)
			{
				case MaximumApiRequestsReachedEventArgs:
					var isAccepted = await DisplayAlert(eventArgs.Title, eventArgs.Message, eventArgs.Accept, eventArgs.Cancel);
					if (isAccepted)
						await _deepLinkingService.OpenBrowser(GitHubConstants.GitHubRateLimitingDocs, CancellationToken.None);
					break;

				case AbuseLimitPullToRefreshEventArgs when _gitHubUserService.GitHubApiAbuseLimitCount <= 1:
					var isAlertAccepted = await DisplayAlert(eventArgs.Title, eventArgs.Message, eventArgs.Accept, eventArgs.Cancel);
					if (isAlertAccepted)
						await _deepLinkingService.OpenBrowser(GitHubConstants.GitHubApiAbuseDocs, CancellationToken.None);
					break;

				case AbuseLimitPullToRefreshEventArgs:
					// Don't display error message when GitHubUserService.GitHubApiAbuseLimitCount > 1
					break;

				case LoginExpiredPullToRefreshEventArgs:
					await DisplayAlert(eventArgs.Title, eventArgs.Message, eventArgs.Cancel);
					await NavigateToWelcomePage();
					break;

				case ErrorPullToRefreshEventArgs:
					await DisplayAlert(eventArgs.Title, eventArgs.Message, eventArgs.Cancel);
					break;

				default:
					throw new NotSupportedException();
			}
		}
	});

	void HandlePreferredLanguageChanged(object? sender, string? e)
	{
		var sortItem = ToolbarItems.First(static x => x.AutomationId is RepositoryPageAutomationIds.SortButton);
		var settingsItem = ToolbarItems.First(static x => x.AutomationId is RepositoryPageAutomationIds.SettingsButton);

		sortItem.Text = RepositoryPageConstants.SortToolbarItemText;
		settingsItem.Text = PageTitles.SettingsPage;
	}

	void HandleSearchBarTextChanged(object? sender, string searchBarText) => BindingContext.SetSearchBarTextCommand.Execute(searchBarText);

	void ISearchPage.OnSearchBarTextChanged(in string text) => _searchTextChangedEventManager.RaiseEvent(this, text, nameof(SearchBarTextChanged));
}