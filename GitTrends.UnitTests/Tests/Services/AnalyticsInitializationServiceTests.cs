using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
	class AnalyticsInitializationServiceTests : BaseTest
	{
		[Test]
		public async Task InitalizeTest()
		{
			//Arrange
			bool isConfigured_Initial, isConfigured_Final;
			var analyticsService = ServiceCollection.ServiceProvider.GetRequiredService<IAnalyticsService>();
			var analyticsInitializationService = ServiceCollection.ServiceProvider.GetRequiredService<AnalyticsInitializationService>();

			//Act
			isConfigured_Initial = analyticsService.Configured;

			await analyticsInitializationService.Initialize(CancellationToken.None).ConfigureAwait(false);

			isConfigured_Final = analyticsService.Configured;

			//Assert
#if AppStore
            Assert.IsFalse(isConfigured_Initial);
#else
			Assert.IsTrue(isConfigured_Initial);
#endif
			Assert.IsTrue(isConfigured_Final);
		}
	}
}