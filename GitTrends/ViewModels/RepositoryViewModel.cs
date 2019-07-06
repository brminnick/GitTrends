using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        public ObservableCollection<Repository> RepositoryCollection { get; } = new ObservableCollection<Repository>();

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
                var gitHubRepositoryList = await GitHubGraphQLApiService.GetRepositories(repositoryOwner).ConfigureAwait(false);
                var currentRepositoryCollectionUriList = RepositoryCollection.Select(x => x.Uri);

                var repositoriesToAdd = gitHubRepositoryList.Where(x => !currentRepositoryCollectionUriList.Contains(x.Uri));

                var sortedRepositoryToAddList = repositoriesToAdd.Where(x => x.Owner.Login.Equals(repositoryOwner, StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.StarCount);

                foreach (var repository in sortedRepositoryToAddList)
                    RepositoryCollection.Add(repository);
            }
            catch (ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.Unauthorized)
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

        void OnPullToRefreshFailed(string title, string message) =>
            _pullToRefreshFailedEventManager.HandleEvent(this, new PullToRefreshFailedEventArgs(message, title), nameof(PullToRefreshFailed));
        #endregion
    }
}
