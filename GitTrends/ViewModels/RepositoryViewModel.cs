using System;
using System.Collections;
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

namespace GitTrends
{
    public class RepositoryViewModel : BaseViewModel
    {
        readonly WeakEventManager<PullToRefreshFailedEventArgs> _pullToRefreshFailedEventManager = new WeakEventManager<PullToRefreshFailedEventArgs>();

        bool _isRefreshing;

        public RepositoryViewModel()
        {
            PullToRefreshCommand = new AsyncCommand(() => ExecutePullToRefreshCommand(GitHubAuthenticationService.Alias));
        }

        public event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        {
            add => _pullToRefreshFailedEventManager.AddEventHandler(value);
            remove => _pullToRefreshFailedEventManager.RemoveEventHandler(value);
        }

        public ICommand PullToRefreshCommand { get; }

        public ObservableCollection<Repository> RepositoryCollection { get; } = new ObservableCollection<Repository>();

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
                var repositoryList = await GitHubGraphQLApiService.GetRepositories(repositoryOwner).ConfigureAwait(false);

                AddRepositoriesToCollection(repositoryList, repositoryOwner);

                RepositoryDatabase.SaveRepositories(repositoryList).SafeFireAndForget();
            }
            catch (ApiException e) when (e.StatusCode is HttpStatusCode.Unauthorized)
            {
                OnPullToRefreshFailed("Login Expired", "Login again");

                await GitHubAuthenticationService.LogOut().ConfigureAwait(false);

                RepositoryCollection.Clear();
            }
            catch (ApiException)
            {
                var repositoryList = await RepositoryDatabase.GetRepositories().ConfigureAwait(false);

                AddRepositoriesToCollection(repositoryList, repositoryOwner);
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

        void AddRepositoriesToCollection(in IEnumerable<Repository> repositories, string repositoryOwner)
        {
            var sortedNewRepositoryList = repositories.Where(x => x.OwnerLogin.Equals(repositoryOwner, StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.StarCount).ToList();

            RepositoryCollection.Clear();

            foreach (var repository in sortedNewRepositoryList)
                RepositoryCollection.Add(repository);
        }

        void OnPullToRefreshFailed(string title, string message) =>
            _pullToRefreshFailedEventManager.HandleEvent(this, new PullToRefreshFailedEventArgs(message, title), nameof(PullToRefreshFailed));
    }
}
