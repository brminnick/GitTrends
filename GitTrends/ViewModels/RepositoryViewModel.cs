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
        bool _isRefreshing, _isListViewVisible, _isGitHubLoginButtonVisible = true;
        ObservableCollection<Repository> _repositoryCollection = new ObservableCollection<Repository>();
        #endregion

        public RepositoryViewModel()
        {
            GitHubAuthenticationService.AuthorizeSessionCompleted += HandleAuthorizeSessionCompleted;

            LoginButtonCommand = new AsyncCommand(ExecuteLoginButtonCommand, continueOnCapturedContext: false);
            PullToRefreshCommand = new AsyncCommand(() => ExecutePullToRefreshCommand("brminnick"), continueOnCapturedContext: false);
        }

        #region Events
        public event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }
        #endregion

        #region Properties
        public ICommand LoginButtonCommand { get; }
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

        public bool IsGitHubLoginButtonVisible
        {
            get => _isGitHubLoginButtonVisible;
            set => SetProperty(ref _isGitHubLoginButtonVisible, value);
        }

        public bool IsListViewVisible
        {
            get => _isListViewVisible;
            set => SetProperty(ref _isListViewVisible, value);
        }
        #endregion

        #region Methods
        async Task ExecutePullToRefreshCommand(string repositoryOwner)
        {
            IsGitHubLoginButtonVisible = false;
            IsListViewVisible = true;

            try
            {
                var repositoryList = await GitHubGraphQLApiService.GetRepositories(repositoryOwner).ConfigureAwait(false);

                foreach (var repository in repositoryList.OrderByDescending(x => x.StarCount))
                {
                    _repositoryCollection.Add(repository);
                }
            }
            catch (ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.Unauthorized)
            {
                OnPullToRefreshFailed("Invalid Api Token", "Login again");
                IsGitHubLoginButtonVisible = true;
                IsListViewVisible = false;
            }
            catch (Exception e)
            {
                OnPullToRefreshFailed("Error", e.Message);
                IsGitHubLoginButtonVisible = true;
                IsListViewVisible = false;
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        Task ExecuteLoginButtonCommand()
        {
            IsGitHubLoginButtonVisible = false;
            return GitHubAuthenticationService.LaunchWebAuthenticationPage();
        }

        void HandleAuthorizeSessionCompleted(object sender, AuthorizeSessionCompletedEventArgs e)
        {
            IsListViewVisible = e.IsSessionAuthorized;
            IsGitHubLoginButtonVisible = !e.IsSessionAuthorized;

            if (e.IsSessionAuthorized)
                PullToRefreshCommand.Execute(null);
        }

        void OnPullToRefreshFailed(string title, string message) =>
            _pullToRefreshFailedEventManager.HandleEvent(this, new PullToRefreshFailedEventArgs(message, title), nameof(PullToRefreshFailed));
        #endregion
    }
}
