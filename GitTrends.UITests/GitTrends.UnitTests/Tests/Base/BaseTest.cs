using System.Threading.Tasks;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    abstract class BaseTest
    {
        [SetUp]
        public virtual Task Setup() => Task.CompletedTask;
    }
}
