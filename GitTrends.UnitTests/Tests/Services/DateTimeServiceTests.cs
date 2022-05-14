using System;
using System.Collections.Generic;
using System.Linq;
using GitTrends.Shared;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
	class DateTimeServiceTests : BaseTest
	{
		[TestCase(100, 1)]
		[TestCase(100, 99)]
		[TestCase(1000, 10)]
		[TestCase(1000, 900)]
		[TestCase(1000000, 52)]
		[TestCase(1000000, 520)]
		public void VerifyGetEstimatedStarredAtList_ExistingData(int totalStarCount, int beginningStarCount)
		{
			// Arrange
			var random = new Random((int)DateTime.Now.Ticks);
			var todaysDate = DateTimeOffset.UtcNow;
			var startingDate = todaysDate.AddMonths(-1);

			var beginningStarredAtList = new List<DateTimeOffset> { startingDate };
			for (int i = 0; i < beginningStarCount - 1; i++)
			{
				var timeSpan = todaysDate - startingDate;
				var randomSpan = new TimeSpan(0, random.Next(0, (int)timeSpan.TotalMinutes), 0);

				var randomDateTimeOffset = startingDate + randomSpan;
				beginningStarredAtList.Add(randomDateTimeOffset);
			}

			Assert.AreEqual(beginningStarCount, beginningStarredAtList.Count);
			Assert.AreEqual(startingDate, beginningStarredAtList.Min());
			Assert.LessOrEqual(beginningStarredAtList.Max(), todaysDate);

			Repository repository = CreateRepository(false) with
			{
				StarredAt = beginningStarredAtList
			};

			// Act
			var estimatedStarredAtList = DateTimeService.GetEstimatedStarredAtList(repository, totalStarCount);

			// Assert
			Assert.AreEqual(totalStarCount, estimatedStarredAtList.Count);

			Assert.AreEqual(startingDate, estimatedStarredAtList.Min());
			Assert.AreEqual(estimatedStarredAtList.Min(), estimatedStarredAtList.First());

			Assert.LessOrEqual(estimatedStarredAtList.Max(), DateTimeOffset.UtcNow.AddMinutes(10));
			Assert.AreEqual(estimatedStarredAtList.Max(), estimatedStarredAtList.Last());
		}

		[TestCase(100)]
		[TestCase(1000)]
		[TestCase(10000)]
		[TestCase(1000000)]
		public void VerifyGetEstimatedStarredAtList_NoData(int totalStarCount)
		{
			// Arrange
			var todaysDate = DateTimeOffset.UtcNow;

			Repository repository = CreateRepository(false);

			// Act
			var estimatedStarredAtList = DateTimeService.GetEstimatedStarredAtList(repository, totalStarCount);

			// Assert
			Assert.AreEqual(totalStarCount, estimatedStarredAtList.Count);

			Assert.LessOrEqual(DateTimeOffset.MinValue, estimatedStarredAtList.Min());
			Assert.AreEqual(estimatedStarredAtList.Min(), estimatedStarredAtList.First());

			Assert.LessOrEqual(estimatedStarredAtList.Max(), DateTimeOffset.UtcNow.AddMinutes(10));
			Assert.AreEqual(estimatedStarredAtList.Max(), estimatedStarredAtList.Last());
		}
	}
}

