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

		[Test]
		public void GetMinimumDateTimeOffsetTest_NoData()
		{
			// Arrange 
			var expectedDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));

			IReadOnlyList<DailyViewsModel> emptyDailyViewsModelList = Array.Empty<DailyViewsModel>();
			IReadOnlyList<DailyClonesModel> emptyDailyClonesModelList = Array.Empty<DailyClonesModel>();

			// Act
			var minimumDailyViewsModel = DateTimeService.GetMinimumDateTimeOffset(emptyDailyViewsModelList);
			var minimumDailyClonesModel = DateTimeService.GetMinimumDateTimeOffset(emptyDailyClonesModelList);

			// Assert
			Assert.AreEqual(expectedDate.Year, minimumDailyViewsModel.Year);
			Assert.AreEqual(expectedDate.Month, minimumDailyViewsModel.Month);
			Assert.AreEqual(expectedDate.Day, minimumDailyViewsModel.Day);

			Assert.AreEqual(expectedDate.Year, minimumDailyClonesModel.Year);
			Assert.AreEqual(expectedDate.Month, minimumDailyClonesModel.Month);
			Assert.AreEqual(expectedDate.Day, minimumDailyClonesModel.Day);
		}

		[Test]
		public void GetMinimumDateTimeOffsetTest_WithData()
		{
			// Arrange 
			var expectedDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));

			var emptyDailyViewsModelList = new List<DailyViewsModel>();
			var emptyDailyClonesModelList = new List<DailyClonesModel>();

			for (var i = expectedDate; i < DateTimeOffset.UtcNow; i = i.AddDays(1))
			{
				emptyDailyViewsModelList.Add(new DailyViewsModel(i, 1, 1));
				emptyDailyClonesModelList.Add(new DailyClonesModel(i, 1, 1));
			}

			// Act
			var minimumDailyViewsModel = DateTimeService.GetMinimumDateTimeOffset(emptyDailyViewsModelList);
			var minimumDailyClonesModel = DateTimeService.GetMinimumDateTimeOffset(emptyDailyClonesModelList);

			// Assert
			Assert.AreEqual(expectedDate.Year, minimumDailyViewsModel.Year);
			Assert.AreEqual(expectedDate.Month, minimumDailyViewsModel.Month);
			Assert.AreEqual(expectedDate.Day, minimumDailyViewsModel.Day);

			Assert.AreEqual(expectedDate.Year, minimumDailyClonesModel.Year);
			Assert.AreEqual(expectedDate.Month, minimumDailyClonesModel.Month);
			Assert.AreEqual(expectedDate.Day, minimumDailyClonesModel.Day);
		}

		[Test]
		public void GetMaximumDateTimeOffsetTest_NoData()
		{
			// Arrange 
			var expectedDate = DateTimeOffset.UtcNow;

			IReadOnlyList<DailyViewsModel> emptyDailyViewsModelList = Array.Empty<DailyViewsModel>();
			IReadOnlyList<DailyClonesModel> emptyDailyClonesModelList = Array.Empty<DailyClonesModel>();

			// Act
			var minimumDailyViewsModel = DateTimeService.GetMaximumDateTimeOffset(emptyDailyViewsModelList);
			var minimumDailyClonesModel = DateTimeService.GetMaximumDateTimeOffset(emptyDailyClonesModelList);

			// Assert
			Assert.AreEqual(expectedDate.Year, minimumDailyViewsModel.Year);
			Assert.AreEqual(expectedDate.Month, minimumDailyViewsModel.Month);
			Assert.AreEqual(expectedDate.Day, minimumDailyViewsModel.Day);

			Assert.AreEqual(expectedDate.Year, minimumDailyClonesModel.Year);
			Assert.AreEqual(expectedDate.Month, minimumDailyClonesModel.Month);
			Assert.AreEqual(expectedDate.Day, minimumDailyClonesModel.Day);
		}

		[Test]
		public void GetMaximumDateTimeOffsetTest_WithData()
		{
			// Arrange 
			var expectedDate = DateTimeOffset.UtcNow;

			var emptyDailyViewsModelList = new List<DailyViewsModel>();
			var emptyDailyClonesModelList = new List<DailyClonesModel>();

			for (var i = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13)); i < DateTimeOffset.UtcNow; i = i.AddDays(1))
			{
				emptyDailyViewsModelList.Add(new DailyViewsModel(i, 1, 1));
				emptyDailyClonesModelList.Add(new DailyClonesModel(i, 1, 1));
			}

			// Act
			var minimumDailyViewsModel = DateTimeService.GetMaximumDateTimeOffset(emptyDailyViewsModelList);
			var minimumDailyClonesModel = DateTimeService.GetMaximumDateTimeOffset(emptyDailyClonesModelList);

			// Assert
			Assert.AreEqual(expectedDate.Year, minimumDailyViewsModel.Year);
			Assert.AreEqual(expectedDate.Month, minimumDailyViewsModel.Month);
			Assert.AreEqual(expectedDate.Day, minimumDailyViewsModel.Day);

			Assert.AreEqual(expectedDate.Year, minimumDailyClonesModel.Year);
			Assert.AreEqual(expectedDate.Month, minimumDailyClonesModel.Month);
			Assert.AreEqual(expectedDate.Day, minimumDailyClonesModel.Day);
		}

		[Test]
		public void EnsureHourMinutesSecondsRemoved()
		{
			// Arrange
			DateTimeOffset dateTimeOffset_Initial, dateTimeOffset_Final;
			dateTimeOffset_Initial = DateTimeOffset.UtcNow;

			// Act
			dateTimeOffset_Final = dateTimeOffset_Initial.RemoveHourMinuteSecond();

			// Asset
			Assert.AreEqual(dateTimeOffset_Initial.Year, dateTimeOffset_Final.Year);
			Assert.AreEqual(dateTimeOffset_Initial.Month, dateTimeOffset_Final.Month);
			Assert.AreEqual(dateTimeOffset_Initial.Day, dateTimeOffset_Final.Day);
			Assert.AreEqual(0, dateTimeOffset_Final.Hour);
			Assert.AreEqual(0, dateTimeOffset_Final.Minute);
			Assert.AreEqual(0, dateTimeOffset_Final.Second);
			Assert.AreEqual(0, dateTimeOffset_Final.Millisecond);
		}
	}
}