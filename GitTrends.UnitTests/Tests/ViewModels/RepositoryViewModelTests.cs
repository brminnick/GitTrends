using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    public class RepositoryViewModelTests
    {
        //public event EventHandler<PullToRefreshFailedEventArgs> PullToRefreshFailed
        //public ICommand PullToRefreshCommand { get; }
        //public ICommand FilterRepositoriesCommand { get; }
        //public ICommand SortRepositoriesCommand { get; }
        //public IReadOnlyList<Repository> VisibleRepositoryList

        //public string EmptyDataViewTitle
        //public string EmptyDataViewDescription

        //public bool IsRefreshing

        [Test]
        public async Task FilterRepositoriesCommandTest()
        {
            //Arrange
            var repositoryViewModel = ServiceCollection.ServiceProvider.GetService<RepositoryViewModel>();


            //Act

            //Assert
        }

    }
}
