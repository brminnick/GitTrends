using System;
using System.Collections.Generic;
using System.Linq;
using GitTrends.Shared;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
	class DateTimeServiceTests : BaseTest
	{
		[Test]
		public void VerifyGetEstimatedStarredAtList_NoData()
		{
			// Arrange
			const int starCount = 100;
			var startingDate = DateTimeOffset.UtcNow.AddMonths(-1);

			Repository repository = CreateRepository(false) with
			{
				StarredAt = new List<DateTimeOffset>()
				{
					startingDate
				}
			};

			// Act
			var estimatedStarredAtList = DateTimeService.GetEstimatedStarredAtList(repository, starCount);

			// Assert
			Assert.AreEqual(starCount, estimatedStarredAtList.Count);

			Assert.AreEqual(startingDate, estimatedStarredAtList.Min());
			Assert.AreEqual(estimatedStarredAtList.Min(), estimatedStarredAtList.First());

			Assert.LessOrEqual(estimatedStarredAtList.Max(), DateTimeOffset.UtcNow);
			Assert.LessOrEqual(estimatedStarredAtList.Max(), estimatedStarredAtList.Last());
		}
	}
}

