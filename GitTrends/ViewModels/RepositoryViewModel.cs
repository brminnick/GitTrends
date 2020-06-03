using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using Autofac;
using GitTrends.Mobile.Shared;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class RepositoryViewModel : BaseViewModel
    {
        readonly WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new WeakEventManager<PullToRefreshFailedEventArgs>();
        readonly RepositoryDatabase _repositoryDatabase;
        readonly GitHubAuthenticationService _gitHubAuthenticationService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly SortingService _sortingService;
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly GitHubUserService _gitHubUserService;

        bool _isRefreshing;
        string _searchBarText = string.Empty;
        string _emptyDataViewTitle = string.Empty;
        string _emptyDataViewDescription = string.Empty;
        IReadOnlyList<Repository> _repositoryList = Enumerable.Empty<Repository>().ToList();
        IReadOnlyList<Repository> _visibleRepositoryList = Enumerable.Empty<Repository>().ToList();

        public RepositoryViewModel(RepositoryDatabase repositoryDatabase,
                                    GitHubAuthenticationService gitHubAuthenticationService,
                                    GitHubGraphQLApiService gitHubGraphQLApiService,
                                    IAnalyticsService analyticsService,
                                    SortingService sortingService,
                                    GitHubApiV3Service gitHubApiV3Service,
                                    NotificationService notificationService,
                                    IMainThread mainThread,
                                    GitHubUserService gitHubUserService) : base(analyticsService, mainThread)
        {
            _repositoryDatabase = repositoryDatabase;
            _gitHubAuthenticationService = gitHubAuthenticationService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _sortingService = sortingService;
            _gitHubApiV3Service = gitHubApiV3Service;
            _gitHubUserService = gitHubUserService;

            RefreshState = RefreshState.Uninitialized;

            PullToRefreshCommand = new AsyncCommand(() => ExecutePullToRefreshCommand(gitHubUserService.Alias));
            FilterRepositoriesCommand = new Command<string>(SetSearchBarText);
            SortRepositoriesCommand = new Command<SortingOption>(ExecuteSortRepositoriesCommand);

            notificationService.SortingOptionRequested += HandleSortingOptionRequested;
            gitHubAuthenticationService.LoggedOut += HandleGitHubAuthenticationServiceLoggedOut;
            gitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;
            gitHubAuthenticationService.DemoUserActivated += HandleDemoUserActivated;
        }

        public event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }

        public IAsyncCommand PullToRefreshCommand { get; }
        public ICommand FilterRepositoriesCommand { get; }
        public ICommand SortRepositoriesCommand { get; }

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

        RefreshState RefreshState
        {
            set
            {
                EmptyDataViewTitle = EmptyDataViewConstants.GetRepositoryTitleText(value, !_repositoryList.Any());
                EmptyDataViewDescription = EmptyDataViewConstants.GetRepositoryDescriptionText(value, !_repositoryList.Any());
            }
        }

        async Task ExecutePullToRefreshCommand(string repositoryOwner)
        {
            HttpResponseMessage? finalResponse = null;

            var cancellationTokenSource = new CancellationTokenSource();
            _gitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;
            _gitHubAuthenticationService.LoggedOut += HandleLoggedOut;

            AnalyticsService.Track("Refresh Triggered", "Sorting Option", _sortingService.CurrentOption.ToString());

            try
            {
                await foreach (var retrievedRepositories in _gitHubGraphQLApiService.GetRepositories(repositoryOwner, cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    //Only display the first update to avoid unncessary work on the UIThread
                    var shouldUpdateVisibleRepositoryList = !VisibleRepositoryList.Any();
                    AddRepositoriesToCollection(retrievedRepositories, _searchBarText, shouldUpdateVisibleRepositoryList);
                }

                var completedRepoitories = new List<Repository>();
                await foreach (var retrievedRepositoryWithViewsAndClonesData in _gitHubApiV3Service.UpdateRepositoriesWithViewsAndClonesData(_repositoryList, cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    completedRepoitories.Add(retrievedRepositoryWithViewsAndClonesData);

                    //Batch the VisibleRepositoryList Updates to avoid overworking the UI Thread
                    if (!_gitHubUserService.IsDemoUser && completedRepoitories.Count > 20)
                    {
                        AddRepositoriesToCollection(completedRepoitories, _searchBarText);
                        completedRepoitories.Clear();
                    }
                }

                //Add Remaining Repositories to VisibleRepositoryList
                AddRepositoriesToCollection(completedRepoitories, _searchBarText, shouldRemoveRepoisitoriesWithoutViewsClonesData: true);

                if (!_gitHubUserService.IsDemoUser)
                {
                    //Call EnsureSuccessStatusCode to confirm the above API calls executed successfully
                    finalResponse = await _gitHubApiV3Service.GetGitHubApiResponse(cancellationTokenSource.Token).ConfigureAwait(false);
                    finalResponse.EnsureSuccessStatusCode();
                }

                RefreshState = RefreshState.Succeeded;
            }
            catch (Exception e) when ((e is ApiException exception && exception.StatusCode is HttpStatusCode.Unauthorized)
                                        || (e is HttpRequestException && finalResponse != null && finalResponse.StatusCode is HttpStatusCode.Unauthorized))
            {
                var loginExpiredEventArgs = new LoginExpiredPullToRefreshEventArgs();

                OnPullToRefreshFailed(new LoginExpiredPullToRefreshEventArgs());

                await _gitHubAuthenticationService.LogOut().ConfigureAwait(false);
                await _repositoryDatabase.DeleteAllData().ConfigureAwait(false);

                SetRepositoriesCollection(Enumerable.Empty<Repository>(), _searchBarText);

                RefreshState = RefreshState.LoginExpired;
            }
            catch (Exception e) when (GitHubApiService.HasReachedMaximimApiCallLimit(e)
                                        || (e is HttpRequestException && finalResponse != null && GitHubApiService.HasReachedMaximimApiCallLimit(finalResponse.Headers)))
            {
                var responseHeaders = e switch
                {
                    ApiException exception => exception.Headers,
                    HttpRequestException _ when finalResponse != null => finalResponse.Headers,
                    _ => throw new NotSupportedException()
                };

                var maximimApiRequestsReachedEventArgs = new MaximimApiRequestsReachedEventArgs(GitHubApiService.GetRateLimitResetDateTime(responseHeaders));
                OnPullToRefreshFailed(maximimApiRequestsReachedEventArgs);

                SetRepositoriesCollection(Enumerable.Empty<Repository>(), _searchBarText);

                RefreshState = RefreshState.MaximumApiLimit;
            }
            catch (Exception e)
            {
                AnalyticsService.Report(e);

                var repositoryList = await _repositoryDatabase.GetRepositories().ConfigureAwait(false);
                SetRepositoriesCollection(repositoryList, _searchBarText);

                if (repositoryList.Any())
                {
                    var dataDownloadedAt = repositoryList.Max(x => x.DataDownloadedAt);
                    OnPullToRefreshFailed(new ErrorPullToRefreshEventArgs($"Displaying data from {dataDownloadedAt.ToLocalTime():dd MMMM @ HH:mm}\n\nCheck your internet connection and try again."));
                }
                else
                {
                    OnPullToRefreshFailed(new ErrorPullToRefreshEventArgs($"Check your internet connection and try again"));
                }

                RefreshState = RefreshState.Error;
            }
            finally
            {
                _gitHubAuthenticationService.LoggedOut -= HandleLoggedOut;
                _gitHubAuthenticationService.AuthorizeSessionStarted -= HandleAuthorizeSessionStarted;

                if (cancellationTokenSource.IsCancellationRequested)
                    UpdateListForLoggedOutUser();

                IsRefreshing = false;

                SaveRepositoriesToDatabase(_repositoryList).SafeFireAndForget();
            }

            void HandleLoggedOut(object sender, EventArgs e) => cancellationTokenSource.Cancel();
            void HandleAuthorizeSessionStarted(object sender, EventArgs e) => cancellationTokenSource.Cancel();
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
            if (_sortingService.CurrentOption == option)
                _sortingService.IsReversed = !_sortingService.IsReversed;
            else
                _sortingService.IsReversed = false;

            _sortingService.CurrentOption = option;

            AnalyticsService.Track("SortingOption Changed", new Dictionary<string, string>
            {
                { nameof(SortingService) + nameof(SortingService.CurrentOption), _sortingService.CurrentOption.ToString() },
                { nameof(SortingService) + nameof(SortingService.IsReversed), _sortingService.IsReversed.ToString() }
            });

            UpdateVisibleRepositoryList(_searchBarText, _sortingService.CurrentOption, _sortingService.IsReversed);
        }

        void SetRepositoriesCollection(in IEnumerable<Repository> repositories, string searchBarText)
        {
            _repositoryList = repositories.ToList();

            UpdateVisibleRepositoryList(searchBarText, _sortingService.CurrentOption, _sortingService.IsReversed);
        }

        void AddRepositoriesToCollection(IEnumerable<Repository> repositories, string searchBarText, bool shouldUpdateVisibleRepositoryList = true, bool shouldRemoveRepoisitoriesWithoutViewsClonesData = false)
        {
            var updatedRepositoryList = _repositoryList.Concat(repositories);

            if (shouldRemoveRepoisitoriesWithoutViewsClonesData)
                _repositoryList = RepositoryService.RemoveForksAndDuplicates(updatedRepositoryList).Where(x => x.DailyClonesList.Count > 1 || x.DailyViewsList.Count > 1).ToList();
            else
                _repositoryList = RepositoryService.RemoveForksAndDuplicates(updatedRepositoryList).ToList();

            if (shouldUpdateVisibleRepositoryList)
                UpdateVisibleRepositoryList(searchBarText, _sortingService.CurrentOption, _sortingService.IsReversed);
        }

        void UpdateVisibleRepositoryList(in string searchBarText, in SortingOption sortingOption, in bool isReversed)
        {
            var filteredRepositoryList = GetRepositoriesFilteredBySearchBar(_repositoryList, searchBarText);

            VisibleRepositoryList = SortingService.SortRepositories(filteredRepositoryList, sortingOption, isReversed).ToList();
        }

        void UpdateListForLoggedOutUser()
        {
            _repositoryList = Enumerable.Empty<Repository>().ToList();
            UpdateVisibleRepositoryList(string.Empty, _sortingService.CurrentOption, _sortingService.IsReversed);
        }

        IEnumerable<Repository> GetRepositoriesFilteredBySearchBar(in IEnumerable<Repository> repositories, string searchBarText)
        {
            if (string.IsNullOrWhiteSpace(searchBarText))
                return repositories;

            return repositories.Where(x => x.Name.Contains(searchBarText, StringComparison.OrdinalIgnoreCase));
        }

        void SetSearchBarText(string text)
        {
            if (EqualityComparer<string>.Default.Equals(_searchBarText, text))
                return;

            _searchBarText = text;

            if (_repositoryList.Any())
                UpdateVisibleRepositoryList(_searchBarText, _sortingService.CurrentOption, _sortingService.IsReversed);
        }

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
                MaximimApiRequestsReachedEventArgs _ => RefreshState.MaximumApiLimit,
                LoginExpiredPullToRefreshEventArgs _ => RefreshState.LoginExpired,
                _ => throw new NotSupportedException()
            };

            _pullToRefreshFailedEventManager.HandleEvent(this, pullToRefreshFailedEventArgs, nameof(PullToRefreshFailed));
        }
    }
}
