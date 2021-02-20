using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using Autofac;
using GitHubApiStatus;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class RepositoryViewModel : BaseViewModel
    {
        readonly static WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new();

        readonly ImageCachingService _imageService;
        readonly GitHubUserService _gitHubUserService;
        readonly RepositoryDatabase _repositoryDatabase;
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly MobileSortingService _mobileSortingService;
        readonly GitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly GitHubAuthenticationService _gitHubAuthenticationService;
        readonly GitHubApiRepositoriesService _gitHubApiRepositoriesService;

        bool _isRefreshing;
        string _titleText = string.Empty;
        string _searchBarText = string.Empty;
        string _emptyDataViewTitle = string.Empty;
        string _emptyDataViewDescription = string.Empty;

        string _totalButtonText = string.Empty;

        public string TotalButtonText
        {
            get { return _totalButtonText; }
            set => SetProperty(ref _totalButtonText, value);
        }


        IReadOnlyList<Repository> _repositoryList = Array.Empty<Repository>();
        IReadOnlyList<Repository> _visibleRepositoryList = Array.Empty<Repository>();

        public RepositoryViewModel(IMainThread mainThread,
                                    ImageCachingService imageService,
                                    IAnalyticsService analyticsService,
                                    GitHubUserService gitHubUserService,
                                    MobileSortingService sortingService,
                                    RepositoryDatabase repositoryDatabase,
                                    GitHubApiV3Service gitHubApiV3Service,
                                    GitHubApiStatusService gitHubApiStatusService,
                                    GitHubGraphQLApiService gitHubGraphQLApiService,
                                    GitHubAuthenticationService gitHubAuthenticationService,
                                    GitHubApiRepositoriesService gitHubApiRepositoriesService) : base(analyticsService, mainThread)
        {
            LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

            SetTitleText();

            _imageService = imageService;
            _gitHubUserService = gitHubUserService;
            _mobileSortingService = sortingService;
            _repositoryDatabase = repositoryDatabase;
            _gitHubApiV3Service = gitHubApiV3Service;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _gitHubAuthenticationService = gitHubAuthenticationService;
            _gitHubApiRepositoriesService = gitHubApiRepositoriesService;

            // Text of the total button
            _totalButtonText = "To";//"IOMLAN";

            RefreshState = RefreshState.Uninitialized;

            FilterRepositoriesCommand = new Command<string>(SetSearchBarText);
            SortRepositoriesCommand = new Command<SortingOption>(ExecuteSortRepositoriesCommand);

            PullToRefreshCommand = new AsyncCommand(() => ExecutePullToRefreshCommand(gitHubUserService.Alias));

            ToggleIsFavoriteCommand = new AsyncValueCommand<Repository>(repository => ExecuteToggleIsFavoriteCommand(repository));

            NotificationService.SortingOptionRequested += HandleSortingOptionRequested;

            GitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;
            GitHubAuthenticationService.LoggedOut += HandleGitHubAuthenticationServiceLoggedOut;
            GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
        }

        public static event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }

        public ICommand SortRepositoriesCommand { get; }
        public ICommand FilterRepositoriesCommand { get; }
        public IAsyncCommand PullToRefreshCommand { get; }
        public AsyncValueCommand<Repository> ToggleIsFavoriteCommand { get; }

        public IReadOnlyList<Repository> VisibleRepositoryList
        {
            get => _visibleRepositoryList;
            set => SetProperty(ref _visibleRepositoryList, value);
        }

        public string EmptyDataViewTitle
        {
            get => _emptyDataViewTitle;
            set => SetProperty(ref _emptyDataViewTitle, value);
        }


        public FloatingActionButtonSize TotalButtonSize => (TotalButtonText.Length) switch
        {
            <= 3 => FloatingActionButtonSize.Mini,
            <= 5 => FloatingActionButtonSize.Normal,
            > 5 => FloatingActionButtonSize.Large
        };

        public string EmptyDataViewDescription
        {
            get => _emptyDataViewDescription;
            set => SetProperty(ref _emptyDataViewDescription, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public string TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value);
        }

        RefreshState RefreshState
        {
            set
            {
                EmptyDataViewTitle = EmptyDataViewService.GetRepositoryTitleText(value, !_repositoryList.Any());
                EmptyDataViewDescription = EmptyDataViewService.GetRepositoryDescriptionText(value, !_repositoryList.Any());
            }
        }

        static IEnumerable<Repository> GetRepositoriesFilteredBySearchBar(in IEnumerable<Repository> repositories, string searchBarText)
        {
            if (string.IsNullOrWhiteSpace(searchBarText))
                return repositories;

            return repositories.Where(x => x.Name.Contains(searchBarText, StringComparison.OrdinalIgnoreCase));
        }

        async Task ExecutePullToRefreshCommand(string repositoryOwner)
        {
            HttpResponseMessage? finalResponse = null;

            var cancellationTokenSource = new CancellationTokenSource();
            GitHubAuthenticationService.LoggedOut += HandleLoggedOut;
            GitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;

            AnalyticsService.Track("Refresh Triggered", "Sorting Option", _mobileSortingService.CurrentOption.ToString());

            try
            {
                const int minimumBatchCount = 20;

                var favoriteRepositoryUrls = await _repositoryDatabase.GetFavoritesUrls().ConfigureAwait(false);

                var repositoryList = new List<Repository>();
                await foreach (var repository in _gitHubGraphQLApiService.GetRepositories(repositoryOwner, cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    if (favoriteRepositoryUrls.Contains(repository.Url))
                        repositoryList.Add(repository with { IsFavorite = true });
                    else
                        repositoryList.Add(repository);

                    //Batch the VisibleRepositoryList Updates to avoid overworking the UI Thread
                    if (!_gitHubUserService.IsDemoUser && repositoryList.Count > minimumBatchCount)
                    {
                        //Only display the first update to avoid unncessary work on the UIThread
                        var shouldUpdateVisibleRepositoryList = !VisibleRepositoryList.Any();
                        AddRepositoriesToCollection(repositoryList, _searchBarText, shouldUpdateVisibleRepositoryList);
                        repositoryList.Clear();
                    }
                }

                //Add Remaining Repositories to _repositoryList
                AddRepositoriesToCollection(repositoryList, _searchBarText);

                var completedRepositories = new List<Repository>();
                await foreach (var retrievedRepositoryWithViewsAndClonesData in _gitHubApiRepositoriesService.UpdateRepositoriesWithViewsClonesAndStarsData(_repositoryList, cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    completedRepositories.Add(retrievedRepositoryWithViewsAndClonesData);

                    //Batch the VisibleRepositoryList Updates to avoid overworking the UI Thread
                    if (!_gitHubUserService.IsDemoUser && completedRepositories.Count > minimumBatchCount)
                    {
                        AddRepositoriesToCollection(completedRepositories, _searchBarText);
                        completedRepositories.Clear();
                    }
                }

                //Add Remaining Repositories to VisibleRepositoryList
                AddRepositoriesToCollection(completedRepositories, _searchBarText);

                if (!_gitHubUserService.IsDemoUser)
                {
                    //Call EnsureSuccessStatusCode to confirm the above API calls executed successfully
                    finalResponse = await _gitHubApiV3Service.GetGitHubApiResponse(cancellationTokenSource.Token).ConfigureAwait(false);
                    finalResponse.EnsureSuccessStatusCode();
                }

                RefreshState = RefreshState.Succeeded;
            }
            catch (Exception e) when ((e is ApiException exception && exception.StatusCode is HttpStatusCode.Unauthorized)
                                        || (e is HttpRequestException && finalResponse?.StatusCode is HttpStatusCode.Unauthorized))
            {
                var loginExpiredEventArgs = new LoginExpiredPullToRefreshEventArgs();

                OnPullToRefreshFailed(new LoginExpiredPullToRefreshEventArgs());

                await _gitHubAuthenticationService.LogOut().ConfigureAwait(false);
                await _repositoryDatabase.DeleteAllData().ConfigureAwait(false);

                SetRepositoriesCollection(Array.Empty<Repository>(), _searchBarText);

                RefreshState = RefreshState.LoginExpired;
            }
            catch (Exception e) when (_gitHubApiStatusService.HasReachedMaximumApiCallLimit(e)
                                        || (e is HttpRequestException && finalResponse != null && _gitHubApiStatusService.HasReachedMaximimApiCallLimit(finalResponse.Headers)))
            {
                var responseHeaders = e switch
                {
                    ApiException exception => exception.Headers,
                    GraphQLException graphQLException => graphQLException.ResponseHeaders,
                    HttpRequestException when finalResponse != null => finalResponse.Headers,
                    _ => throw new NotSupportedException()
                };

                var maximimApiRequestsReachedEventArgs = new MaximumApiRequestsReachedEventArgs(_gitHubApiStatusService.GetRateLimitResetDateTime(responseHeaders));
                OnPullToRefreshFailed(maximimApiRequestsReachedEventArgs);

                SetRepositoriesCollection(Array.Empty<Repository>(), _searchBarText);

                RefreshState = RefreshState.MaximumApiLimit;
            }
            catch (Exception e)
            {
                AnalyticsService.Report(e, new Dictionary<string, string>
                {
                    { nameof(IGitHubApiStatusService.IsAbuseRateLimit), _gitHubApiStatusService.IsAbuseRateLimit(e, out var delta).ToString() },
                    { nameof(delta), delta.ToString() }
                });

                var repositoryDatabaseList = await _repositoryDatabase.GetRepositories().ConfigureAwait(false);
                SetRepositoriesCollection(repositoryDatabaseList, _searchBarText);

                if (repositoryDatabaseList.Any())
                {
                    var dataDownloadedAt = repositoryDatabaseList.Max(x => x.DataDownloadedAt);
                    OnPullToRefreshFailed(new ErrorPullToRefreshEventArgs($"{RepositoryPageConstants.DisplayingDataFrom} {dataDownloadedAt.ToLocalTime():dd MMMM @ HH:mm}\n\n{RepositoryPageConstants.CheckInternetConnectionTryAgain}."));
                }
                else
                {
                    OnPullToRefreshFailed(new ErrorPullToRefreshEventArgs(RepositoryPageConstants.CheckInternetConnectionTryAgain));
                }

                RefreshState = RefreshState.Error;
            }
            finally
            {
                GitHubAuthenticationService.LoggedOut -= HandleLoggedOut;
                GitHubAuthenticationService.AuthorizeSessionStarted -= HandleAuthorizeSessionStarted;

                if (cancellationTokenSource.IsCancellationRequested)
                    UpdateListForLoggedOutUser();

                IsRefreshing = false;

                SaveRepositoriesToDatabase(_repositoryList).SafeFireAndForget();
            }

            void HandleLoggedOut(object sender, EventArgs e) => cancellationTokenSource.Cancel();
            void HandleAuthorizeSessionStarted(object sender, EventArgs e) => cancellationTokenSource.Cancel();
        }

        async ValueTask ExecuteToggleIsFavoriteCommand(Repository repository)
        {
            var updatedRepository = repository with
            {
                IsFavorite = repository.IsFavorite.HasValue ? !repository.IsFavorite : true
            };

            AnalyticsService.Track("IsFavorite Toggled", nameof(Repository.IsFavorite), updatedRepository.IsFavorite.ToString());

            var updatedRepositoryList = new List<Repository>(_visibleRepositoryList);
            updatedRepositoryList.Remove(repository);
            updatedRepositoryList.Add(updatedRepository);

            SetRepositoriesCollection(updatedRepositoryList, _searchBarText);

            if (!_gitHubUserService.IsDemoUser)
                await _repositoryDatabase.SaveRepository(updatedRepository).ConfigureAwait(false);
        }

        async ValueTask SaveRepositoriesToDatabase(IEnumerable<Repository> repositories)
        {
            if (_gitHubUserService.IsDemoUser)
                return;

            foreach (var repository in repositories)
            {
                try
                {
                    await _repositoryDatabase.SaveRepository(repository).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    AnalyticsService.Report(e);
                }
            }
        }

        void ExecuteSortRepositoriesCommand(SortingOption option)
        {
            if (_mobileSortingService.CurrentOption == option)
                _mobileSortingService.IsReversed = !_mobileSortingService.IsReversed;
            else
                _mobileSortingService.IsReversed = false;

            _mobileSortingService.CurrentOption = option;

            AnalyticsService.Track("SortingOption Changed", new Dictionary<string, string>
            {
                { nameof(MobileSortingService) + nameof(MobileSortingService.CurrentOption), _mobileSortingService.CurrentOption.ToString() },
                { nameof(MobileSortingService) + nameof(MobileSortingService.IsReversed), _mobileSortingService.IsReversed.ToString() }
            });

            UpdateVisibleRepositoryList(_searchBarText, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
        }

        void SetRepositoriesCollection(in IReadOnlyList<Repository> repositories, in string searchBarText)
        {
            _repositoryList = repositories;

            UpdateVisibleRepositoryList(searchBarText, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
        }

        void AddRepositoriesToCollection(in IEnumerable<Repository> repositories, in string searchBarText, in bool shouldUpdateVisibleRepositoryList = true)
        {
            var updatedRepositoryList = _repositoryList.Concat(repositories);
            _repositoryList = RepositoryService.RemoveForksAndDuplicates(updatedRepositoryList).ToList();

            if (shouldUpdateVisibleRepositoryList)
                UpdateVisibleRepositoryList(searchBarText, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
        }

        void UpdateVisibleRepositoryList(in string searchBarText, in SortingOption sortingOption, in bool isReversed)
        {
            var filteredRepositoryList = GetRepositoriesFilteredBySearchBar(_repositoryList, searchBarText);

            VisibleRepositoryList = MobileSortingService.SortRepositories(filteredRepositoryList, sortingOption, isReversed).ToList();

            _imageService.PreloadRepositoryImages(VisibleRepositoryList).SafeFireAndForget(ex => AnalyticsService.Report(ex));
        }

        void UpdateListForLoggedOutUser()
        {
            _repositoryList = Array.Empty<Repository>();
            UpdateVisibleRepositoryList(string.Empty, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
        }

        void SetSearchBarText(string text)
        {
            if (EqualityComparer<string>.Default.Equals(_searchBarText, text))
                return;

            _searchBarText = text;

            if (_repositoryList.Any())
                UpdateVisibleRepositoryList(_searchBarText, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
        }

        void HandlePreferredLanguageChanged(object sender, string? e) => SetTitleText();

        void SetTitleText() => TitleText = PageTitles.RepositoryPage;

        //Work-around because ContentPage.OnAppearing does not fire after `ContentPage.PushModalAsync()`
        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e) => IsRefreshing |= e.IsSessionAuthorized;

        void HandleDemoUserActivated(object sender, EventArgs e) => IsRefreshing = true;

        void HandleGitHubAuthenticationServiceLoggedOut(object sender, EventArgs e) => UpdateListForLoggedOutUser();

        void HandleSortingOptionRequested(object sender, SortingOption sortingOption) => SortRepositoriesCommand.Execute(sortingOption);

        void OnPullToRefreshFailed(PullToRefreshFailedEventArgs pullToRefreshFailedEventArgs)
        {
            RefreshState = pullToRefreshFailedEventArgs switch
            {
                ErrorPullToRefreshEventArgs _ => RefreshState.Error,
                MaximumApiRequestsReachedEventArgs _ => RefreshState.MaximumApiLimit,
                LoginExpiredPullToRefreshEventArgs _ => RefreshState.LoginExpired,
                _ => throw new NotSupportedException()
            };

            _pullToRefreshFailedEventManager.RaiseEvent(this, pullToRefreshFailedEventArgs, nameof(PullToRefreshFailed));
        }
    }
}
