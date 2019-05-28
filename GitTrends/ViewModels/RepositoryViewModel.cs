using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        #region Constant Fields
        readonly WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new WeakEventManager<PullToRefreshFailedEventArgs>();
        #endregion

        #region Fields
        bool _isRefreshing;
        ObservableCollection<Repository> _repositoryCollection = new ObservableCollection<Repository>();
        #endregion

        public RepositoryViewModel()
        {
            PullToRefreshCommand = new AsyncCommand(() => ExecutePullToRefreshCommand(GitHubAuthenticationService.Alias), continueOnCapturedContext: false);
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

        public ObservableCollection<Repository> RepositoryCollection
        {
            get => _repositoryCollection;
            set => SetProperty(ref _repositoryCollection, value);
        }

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
                var repositoryList = await GitHubGraphQLApiService.GetRepositories(repositoryOwner).ConfigureAwait(false);

                foreach (var repository in repositoryList.OrderByDescending(x => x.StarCount))
                {
                    RepositoryCollection.Add(repository);
                }
            }
            catch (ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.Unauthorized)
            {
                OnPullToRefreshFailed("Invalid Api Token", "Login again");
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
