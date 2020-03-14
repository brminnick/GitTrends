using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using Autofac;
using GitTrends.Shared;
using Refit;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class RepositoryViewModel : BaseViewModel
    {
        readonly WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new WeakEventManager<PullToRefreshFailedEventArgs>();
        readonly RepositoryDatabase _repositoryDatabase;
        readonly GitHubAuthenticationService _gitHubAuthenticationService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        bool _isRefreshing;
        string _searchBarText = "";
        IReadOnlyList<Repository> _repositoryList = Enumerable.Empty<Repository>().ToList();
        IReadOnlyList<Repository> _visibleRepositoryList = Enumerable.Empty<Repository>().ToList();

        public RepositoryViewModel(RepositoryDatabase repositoryDatabase,
                                    GitHubAuthenticationService gitHubAuthenticationService,
                                    GitHubGraphQLApiService gitHubGraphQLApiService,
                                    AnalyticsService analyticsService) : base(analyticsService)
        {
            _repositoryDatabase = repositoryDatabase;
            _gitHubAuthenticationService = gitHubAuthenticationService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;

            PullToRefreshCommand = new AsyncCommand(() => ExecutePullToRefreshCommand(GitHubAuthenticationService.Alias));
            FilterRepositoriesCommand = new Command<string>(SetSearchBarText);
            SortRepositoriesCommand = new Command<SortingOption>(ExecuteSortRepositoriesCommand);

            _gitHubAuthenticationService.LoggedOut += HandleGitHubAuthenticationServiceLoggedOut;
        }

        public event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }

        public ICommand PullToRefreshCommand { get; }
        public ICommand FilterRepositoriesCommand { get; }
        public ICommand SortRepositoriesCommand { get; }

        public IReadOnlyList<Repository> VisibleRepositoryList
        {
            get => _visibleRepositoryList;
            set => SetProperty(ref _visibleRepositoryList, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        bool IsSortingReversed
        {
            get => Preferences.Get(nameof(IsSortingReversed), false);
            set => Preferences.Set(nameof(IsSortingReversed), value);
        }

        SortingOption CurrentSortingOption
        {
            get => (SortingOption)Preferences.Get(nameof(CurrentSortingOption), (int)SortingOption.Stars);
            set => Preferences.Set(nameof(CurrentSortingOption), (int)value);
        }

        async Task ExecutePullToRefreshCommand(string repositoryOwner)
        {
            try
            {
                await foreach (var retrievedRepository in _gitHubGraphQLApiService.GetRepositories(repositoryOwner).ConfigureAwait(false))
                {
                    AddRepositoriesToCollection(retrievedRepository, _searchBarText);
                    _repositoryDatabase.SaveRepositories(retrievedRepository).SafeFireAndForget();
                }
            }
            catch (ApiException e) when (e.StatusCode is HttpStatusCode.Unauthorized)
            {
                OnPullToRefreshFailed("Login Expired", "Please login again");

                await _gitHubAuthenticationService.LogOut().ConfigureAwait(false);
                await _repositoryDatabase.DeleteAllData().ConfigureAwait(false);

                VisibleRepositoryList = Enumerable.Empty<Repository>().ToList();
            }
            catch (ApiException)
            {
                var repositoryList = await _repositoryDatabase.GetRepositories().ConfigureAwait(false);

                SetRepositoriesCollection(repositoryList, _searchBarText);
            }
            catch (Exception e)
            {
                AnalyticsService.Report(e);
                OnPullToRefreshFailed("Error", e.Message);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        void ExecuteSortRepositoriesCommand(SortingOption option)
        {
            if (CurrentSortingOption == option)
                IsSortingReversed = !IsSortingReversed;
            else
                IsSortingReversed = false;

            CurrentSortingOption = option;

            AnalyticsService.Track("Sorting Options Updated", new Dictionary<string, string>
            {
                { nameof(CurrentSortingOption), CurrentSortingOption.ToString() },
                { nameof(IsSortingReversed), IsSortingReversed.ToString() }
            });

            UpdateVisibleRepositoryList(_searchBarText, CurrentSortingOption, IsSortingReversed);
        }

        void SetRepositoriesCollection(in IEnumerable<Repository> repositories, string searchBarText)
        {
            _repositoryList = repositories.ToList();

            UpdateVisibleRepositoryList(searchBarText, CurrentSortingOption, IsSortingReversed);
        }

        void AddRepositoriesToCollection(IEnumerable<Repository> repositories, string searchBarText)
        {
            var updatedRepositoryList = _repositoryList.Concat(repositories);
            _repositoryList = RemoveForksAndDuplicates(updatedRepositoryList).ToList();

            UpdateVisibleRepositoryList(searchBarText, CurrentSortingOption, IsSortingReversed);

            static IEnumerable<Repository> RemoveForksAndDuplicates(in IEnumerable<Repository> repositoriesList) => repositoriesList.Where(x => !x.IsFork).GroupBy(x => x.Name).Select(x => x.First());
        }

        void UpdateVisibleRepositoryList(in string searchBarText, in SortingOption sortingOption, in bool isReversed)
        {
            var filteredRepositoryList = GetRepositoriesFilteredBySearchBar(_repositoryList, searchBarText);

            VisibleRepositoryList = sortingOption switch
            {
                SortingOption.Clones when isReversed => throw new NotImplementedException(),
                SortingOption.Clones => throw new NotImplementedException(),
                SortingOption.Forks when isReversed => filteredRepositoryList.OrderBy(x => x.ForkCount).ToList(),
                SortingOption.Forks => filteredRepositoryList.OrderByDescending(x => x.ForkCount).ToList(),
                SortingOption.Issues when isReversed => filteredRepositoryList.OrderBy(x => x.IssuesCount).ToList(),
                SortingOption.Issues => filteredRepositoryList.OrderByDescending(x => x.IssuesCount).ToList(),
                SortingOption.Stars when isReversed => filteredRepositoryList.OrderBy(x => x.StarCount).ToList(),
                SortingOption.Stars => filteredRepositoryList.OrderByDescending(x => x.StarCount).ToList(),
                SortingOption.UniqueClones when isReversed => throw new NotImplementedException(),
                SortingOption.UniqueClones => throw new NotImplementedException(),
                SortingOption.UniqueViews when isReversed => throw new NotImplementedException(),
                SortingOption.UniqueViews => throw new NotImplementedException(),
                SortingOption.Views when isReversed => throw new NotImplementedException(),
                SortingOption.Views => throw new NotImplementedException(),
                _ => throw new NotSupportedException()
            };
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
                UpdateVisibleRepositoryList(_searchBarText, CurrentSortingOption, IsSortingReversed);
        }

        void HandleGitHubAuthenticationServiceLoggedOut(object sender, EventArgs e)
        {
            _repositoryList = Enumerable.Empty<Repository>().ToList();
            UpdateVisibleRepositoryList(string.Empty, CurrentSortingOption, IsSortingReversed);
        }

        void OnPullToRefreshFailed(in string title, in string message) =>
            _pullToRefreshFailedEventManager.HandleEvent(this, new PullToRefreshFailedEventArgs(message, title), nameof(PullToRefreshFailed));
    }
}
