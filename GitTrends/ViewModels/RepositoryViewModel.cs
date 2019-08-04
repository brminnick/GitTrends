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

        bool _isRefreshing;
        string _searchBarText = "";
        IReadOnlyList<Repository>? _repositoryList;

        public RepositoryViewModel()
        {
            PullToRefreshCommand = new AsyncCommand(() => ExecutePullToRefreshCommand(GitHubAuthenticationService.Alias));
            FilterRepositoriesCommand = new Command<string>(searchBarText => SearchBarText = searchBarText);
        }

        public event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }

        public ICommand PullToRefreshCommand { get; }

        public ICommand FilterRepositoriesCommand { get; }

        public ObservableCollection<Repository> VisibleRepositoryCollection { get; } = new ObservableCollection<Repository>();

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
                    AddRepositoriesToCollection(_repositoryList, GitHubAuthenticationService.Alias, value);
            }
        }

        async Task ExecutePullToRefreshCommand(string repositoryOwner)
        {
            await Task.Yield();

            try
            {
                var repositoryList = await GitHubGraphQLApiService.GetRepositories(repositoryOwner).ConfigureAwait(false);

                AddRepositoriesToCollection(repositoryList, repositoryOwner, _searchBarText);

                RepositoryDatabase.SaveRepositories(repositoryList).SafeFireAndForget();
            }
            catch (ApiException e) when (e.StatusCode is HttpStatusCode.Unauthorized)
            {
                OnPullToRefreshFailed("Login Expired", "Login again");

                await GitHubAuthenticationService.LogOut().ConfigureAwait(false);

                VisibleRepositoryCollection.Clear();
            }
            catch (ApiException)
            {
                var repositoryList = await RepositoryDatabase.GetRepositories().ConfigureAwait(false);

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

            VisibleRepositoryCollection.Clear();

            IEnumerable<Repository> filteredRepositoryList;
            if (string.IsNullOrWhiteSpace(searchBarText))
                filteredRepositoryList = _repositoryList;
            else
                filteredRepositoryList = _repositoryList.Where(x => x.Name.Contains(searchBarText));

            foreach (var repository in filteredRepositoryList)
                VisibleRepositoryCollection.Add(repository);
        }

        void OnPullToRefreshFailed(in string title, in string message) =>
            _pullToRefreshFailedEventManager.HandleEvent(this, new PullToRefreshFailedEventArgs(message, title), nameof(PullToRefreshFailed));
    }
}
