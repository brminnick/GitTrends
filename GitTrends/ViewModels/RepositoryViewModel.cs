using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;
using Refit;

namespace GitTrends
{
    public class RepositoryViewModel : BaseViewModel
    {
        #region Constant Fields
        readonly WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new WeakEventManager<PullToRefreshFailedEventArgs>();
        #endregion

        #region Fields
        bool _isRefreshing;
        #endregion

        public RepositoryViewModel()
        {
            PullToRefreshCommand = new AsyncCommand(() => ExecutePullToRefreshCommand(GitHubAuthenticationService.Alias));
        }

        #region Events
        public event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }
        #endregion

        #region Properties
        public ICommand PullToRefreshCommand { get; }

        public ObservableCollection<Repository> RepositoryCollection { get; private set; } = new ObservableCollection<Repository>();

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }
        #endregion

        #region Methods
        async Task ExecutePullToRefreshCommand(string repositoryOwner)
        {
            try
            {
                await AddRepositoriesToCollectionFromDatabase(repositoryOwner).ConfigureAwait(false);
                await AddRepositoriesToCollectionFromGitHub(repositoryOwner).ConfigureAwait(false);
            }
            catch (ApiException e) when (e.StatusCode is HttpStatusCode.Unauthorized)
            {
                OnPullToRefreshFailed("Login Expired", "Login again");
                await GitHubAuthenticationService.LogOut().ConfigureAwait(false);
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

        async Task AddRepositoriesToCollectionFromDatabase(string repositoryOwner)
        {
            var respositoryList = await RepositoryDatabase.GetRepositories().ConfigureAwait(false);

            AddRepositoriesToCollection(GetRepositoriesNotInCollection(respositoryList), repositoryOwner);
        }

        async Task AddRepositoriesToCollectionFromGitHub(string repositoryOwner)
        {
            var gitHubRepositoryList = await GitHubGraphQLApiService.GetRepositories(repositoryOwner).ConfigureAwait(false);

            var repositoriesToAdd = GetRepositoriesNotInCollection(gitHubRepositoryList);

            AddRepositoriesToCollection(repositoriesToAdd, repositoryOwner);

            await RepositoryDatabase.SaveRepositories(repositoriesToAdd).ConfigureAwait(false);
        }

        IEnumerable<Repository> GetRepositoriesNotInCollection(in IEnumerable<Repository> repositoryList)
        {
            var currentRepositoryCollectionUriList = RepositoryCollection.Select(x => x.Uri).ToList();

            return repositoryList.Where(x => !currentRepositoryCollectionUriList.Contains(x.Uri));
        }

        void AddRepositoriesToCollection(in IEnumerable<Repository> repositoriesToAdd, string repositoryOwner)
        {
            var sortedNewRepositoryList = repositoriesToAdd.Where(x => x.OwnerLogin.Equals(repositoryOwner, StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.StarCount).ToList();

            if (!RepositoryCollection.Any())
            {
                foreach (var repository in sortedNewRepositoryList)
                {
                    RepositoryCollection.Add(repository);
                }
            }
            else
            {
                var repositoryList = RepositoryCollection.ToList();
                repositoryList.AddRange(sortedNewRepositoryList);

                var sortedTotalRepositoryList = repositoryList.Where(x => x.OwnerLogin.Equals(repositoryOwner, StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.StarCount).ToList();

                foreach (var repository in sortedNewRepositoryList)
                {
                    var index = sortedTotalRepositoryList.IndexOf(repository);

                    if (index >= RepositoryCollection.Count)
                        RepositoryCollection.Add(repository);
                    else
                        RepositoryCollection.Insert(index, repository);
                }
            }
        }

        void OnPullToRefreshFailed(string title, string message) =>
            _pullToRefreshFailedEventManager.HandleEvent(this, new PullToRefreshFailedEventArgs(message, title), nameof(PullToRefreshFailed));
        #endregion
    }
}
