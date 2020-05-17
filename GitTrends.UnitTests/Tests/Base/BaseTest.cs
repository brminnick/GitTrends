using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    abstract class BaseTest
    {
        [SetUp]
        public virtual async Task Setup()
        {
            var referringSitesDatabase = ContainerService.Container.GetService<ReferringSitesDatabase>();
            await referringSitesDatabase.DeleteAllData().ConfigureAwait(false);

            var repositoryDatabase = ContainerService.Container.GetService<RepositoryDatabase>();
            await repositoryDatabase.DeleteAllData().ConfigureAwait(false);
        }
    }
}
