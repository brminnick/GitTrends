using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using Autofac;
using FFImageLoading;
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
        readonly static WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new WeakEventManager<PullToRefreshFailedEventArgs>();

        readonly GitHubUserService _gitHubUserService;
        readonly RepositoryDatabase _repositoryDatabase;
        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly MobileSortingService _mobileSortingService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly GitHubAuthenticationService _gitHubAuthenticationService;

        bool _isRefreshing;
        string _titleText = string.Empty;
        string _searchBarText = string.Empty;
        string _emptyDataViewTitle = string.Empty;
        string _emptyDataViewDescription = string.Empty;

        IReadOnlyList<Repository> _repositoryList = new List<Repository>();
        IReadOnlyList<Repository> _visibleRepositoryList = new List<Repository>();

        public RepositoryViewModel(IMainThread mainThread,
                                    IAnalyticsService analyticsService,
                                    GitHubUserService gitHubUserService,
                                    MobileSortingService sortingService,
                                    RepositoryDatabase repositoryDatabase,
                                    GitHubApiV3Service gitHubApiV3Service,
                                    GitHubGraphQLApiService gitHubGraphQLApiService,
                                    GitHubAuthenticationService gitHubAuthenticationService) : base(analyticsService, mainThread)
        {
            LanguageService.PreferredLanguageChanged += HandlePreferredLanguageChanged;

            SetTitleText();

            _gitHubUserService = gitHubUserService;
            _mobileSortingService = sortingService;
            _repositoryDatabase = repositoryDatabase;
            _gitHubApiV3Service = gitHubApiV3Service;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
            _gitHubAuthenticationService = gitHubAuthenticationService;

            RefreshState = RefreshState.Uninitialized;

            PullToRefreshCommand = new AsyncCommand(() => ExecutePullToRefreshCommand(gitHubUserService.Alias));
            FilterRepositoriesCommand = new Command<string>(SetSearchBarText);
            SortRepositoriesCommand = new Command<SortingOption>(ExecuteSortRepositoriesCommand);

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

        async Task ExecutePullToRefreshCommand(string repositoryOwner)
        {
            HttpResponseMessage? finalResponse = null;

            var cancellationTokenSource = new CancellationTokenSource();
            GitHubAuthenticationService.AuthorizeSessionStarted += HandleAuthorizeSessionStarted;
            GitHubAuthenticationService.LoggedOut += HandleLoggedOut;

            AnalyticsService.Track("Refresh Triggered", "Sorting Option", _mobileSortingService.CurrentOption.ToString());

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

        void SetRepositoriesCollection(in IEnumerable<Repository> repositories, string searchBarText)
        {
            _repositoryList = repositories.ToList();

            UpdateVisibleRepositoryList(searchBarText, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
        }

        void AddRepositoriesToCollection(IEnumerable<Repository> repositories, string searchBarText, bool shouldUpdateVisibleRepositoryList = true, bool shouldRemoveRepoisitoriesWithoutViewsClonesData = false)
        {
            var updatedRepositoryList = _repositoryList.Concat(repositories);

            if (shouldRemoveRepoisitoriesWithoutViewsClonesData)
                _repositoryList = RepositoryService.RemoveForksAndDuplicates(updatedRepositoryList).Where(x => x.DailyClonesList.Count > 1 || x.DailyViewsList.Count > 1).ToList();
            else
                _repositoryList = RepositoryService.RemoveForksAndDuplicates(updatedRepositoryList).ToList();

            if (shouldUpdateVisibleRepositoryList)
                UpdateVisibleRepositoryList(searchBarText, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
        }

        void UpdateVisibleRepositoryList(in string searchBarText, in SortingOption sortingOption, in bool isReversed)
        {
            var filteredRepositoryList = GetRepositoriesFilteredBySearchBar(_repositoryList, searchBarText);

            VisibleRepositoryList = MobileSortingService.SortRepositories(filteredRepositoryList, sortingOption, isReversed).ToList();

            foreach (var url in VisibleRepositoryList.Select(x => x.OwnerAvatarUrl).Distinct().Where(isValidUri))
                ImageService.Instance.LoadString(url).PreloadAsync().SafeFireAndForget(ex => AnalyticsService.Report(ex));

            static bool isValidUri(string url) => Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        void UpdateListForLoggedOutUser()
        {
            _repositoryList = new List<Repository>();
            UpdateVisibleRepositoryList(string.Empty, _mobileSortingService.CurrentOption, _mobileSortingService.IsReversed);
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
                MaximimApiRequestsReachedEventArgs _ => RefreshState.MaximumApiLimit,
                LoginExpiredPullToRefreshEventArgs _ => RefreshState.LoginExpired,
                _ => throw new NotSupportedException()
            };

            _pullToRefreshFailedEventManager.RaiseEvent(this, pullToRefreshFailedEventArgs, nameof(PullToRefreshFailed));
        }
    }
}
