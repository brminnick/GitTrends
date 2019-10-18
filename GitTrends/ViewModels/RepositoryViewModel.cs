using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;
using Refit;
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
                                    GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _repositoryDatabase = repositoryDatabase;
            _gitHubAuthenticationService = gitHubAuthenticationService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;

            PullToRefreshCommand = new AsyncCommand(() => ExecutePullToRefreshCommand(_gitHubAuthenticationService.Alias));
            FilterRepositoriesCommand = new Command<string>(text => SetSearchBarText(text));
        }

        public event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }

        public ICommand PullToRefreshCommand { get; }
        public ICommand FilterRepositoriesCommand { get; }

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

        async Task ExecutePullToRefreshCommand(string repositoryOwner)
        {
            await Task.Yield();

            try
            {
                await foreach (var retrievedRepository in _gitHubGraphQLApiService.GetRepositories(repositoryOwner).ConfigureAwait(false))
                {
                    AddRepositoriesToCollection(retrievedRepository, repositoryOwner, _searchBarText);
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

                SetRepositoriesCollection(repositoryList, repositoryOwner, _searchBarText);
            }
            catch (Exception e)
            {
                OnPullToRefreshFailed("Error", e.Message);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        void SetRepositoriesCollection(in IEnumerable<Repository> repositories, string loggedInUserGitHubAlias, string searchBarText)
        {
            _repositoryList = repositories.ToList();

            UpdateVisibleRepositoryList(searchBarText);
        }

        void AddRepositoriesToCollection(IEnumerable<Repository> repositories, string loggedInUserGitHubAlias, string searchBarText)
        {
            var updatedRepositoryList = _repositoryList.Concat(repositories);
            _repositoryList = RemoveDuplicatesAndForks(updatedRepositoryList, loggedInUserGitHubAlias).ToList();

            UpdateVisibleRepositoryList(searchBarText);

            IEnumerable<Repository> RemoveDuplicatesAndForks(in IEnumerable<Repository> repositoriesList, string loggedInUser) => repositoriesList.GroupBy(x => x.Name).Select(x => x.FirstOrDefault(x => x.OwnerLogin != loggedInUser) ?? x.First());
        }

        void UpdateVisibleRepositoryList(string searchBarText)
        {
            var filteredRepositoryList = GetRepositoriesFilteredBySearchBar(_repositoryList, searchBarText);
            VisibleRepositoryList = filteredRepositoryList.OrderByDescending(x => x.StarCount).ToList();
        }

        IEnumerable<Repository> GetOwnersRepositories(in IEnumerable<Repository> repositories, string repositoryOwner) =>
            repositories.Where(x => x.OwnerLogin.Equals(repositoryOwner, StringComparison.InvariantCultureIgnoreCase));

        IEnumerable<Repository> GetRepositoriesFilteredBySearchBar(in IEnumerable<Repository> repositories, string searchBarText)
        {
            if (string.IsNullOrWhiteSpace(searchBarText))
                return repositories;

            return repositories.Where(x => x.Name.Contains(searchBarText));
        }

        void SetSearchBarText(in string text)
        {
            if (EqualityComparer<string>.Default.Equals(_searchBarText, text))
                return;

            _searchBarText = text;

            if (_repositoryList.Any())
                UpdateVisibleRepositoryList(_searchBarText);
        }

        void OnPullToRefreshFailed(in string title, in string message) =>
            _pullToRefreshFailedEventManager.HandleEvent(this, new PullToRefreshFailedEventArgs(message, title), nameof(PullToRefreshFailed));
    }
}
