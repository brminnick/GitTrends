using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        IReadOnlyList<Repository>? _repositoryList;

        public RepositoryViewModel(RepositoryDatabase repositoryDatabase,
                                    GitHubAuthenticationService gitHubAuthenticationService,
                                    GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _repositoryDatabase = repositoryDatabase;
            _gitHubAuthenticationService = gitHubAuthenticationService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;

            PullToRefreshCommand = new AsyncCommand(() => ExecutePullToRefreshCommand(_gitHubAuthenticationService.Alias));
            FilterRepositoriesCommand = new Command<string>(searchBarText => SearchBarText = searchBarText);
        }

        public event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }

        public ICommand PullToRefreshCommand { get; }

        public ICommand FilterRepositoriesCommand { get; }

        public ObservableRangeCollection<Repository> VisibleRepositoryCollection { get; } = new ObservableRangeCollection<Repository>();

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        string SearchBarText
        {
            get => _searchBarText;
            set
            {
                _searchBarText = value;

                if (_repositoryList != null && _repositoryList.Any())
                    AddRepositoriesToCollection(_repositoryList, _gitHubAuthenticationService.Alias, value);
            }
        }

        async Task ExecutePullToRefreshCommand(string repositoryOwner)
        {
            await Task.Yield();

            try
            {
                var repositoryList = await _gitHubGraphQLApiService.GetRepositories(repositoryOwner).ConfigureAwait(false);

                AddRepositoriesToCollection(repositoryList, repositoryOwner, _searchBarText);

                _repositoryDatabase.SaveRepositories(repositoryList).SafeFireAndForget();
            }
            catch (ApiException e) when (e.StatusCode is HttpStatusCode.Unauthorized)
            {
                OnPullToRefreshFailed("Login Expired", "Login again");

                await _gitHubAuthenticationService.LogOut().ConfigureAwait(false);

                VisibleRepositoryCollection.Clear();
            }
            catch (ApiException)
            {
                var repositoryList = await _repositoryDatabase.GetRepositories().ConfigureAwait(false);

                AddRepositoriesToCollection(repositoryList, repositoryOwner, _searchBarText);
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

        void AddRepositoriesToCollection(in IEnumerable<Repository> repositories, string repositoryOwner, string searchBarText)
        {
            _repositoryList = repositories.Where(x => x.OwnerLogin.Equals(repositoryOwner, StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.StarCount).ToList();

            IEnumerable<Repository> filteredRepositoryList;
            if (string.IsNullOrWhiteSpace(searchBarText))
                filteredRepositoryList = _repositoryList;
            else
                filteredRepositoryList = _repositoryList.Where(x => x.Name.Contains(searchBarText));

            VisibleRepositoryCollection.Clear();
            VisibleRepositoryCollection.AddRange(filteredRepositoryList);
        }

        void OnPullToRefreshFailed(in string title, in string message) =>
            _pullToRefreshFailedEventManager.HandleEvent(this, new PullToRefreshFailedEventArgs(message, title), nameof(PullToRefreshFailed));
    }
}
