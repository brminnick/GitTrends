using GitTrends.Common;

namespace GitTrends.UnitTests;

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

		var beginningStarredAtList = new List<DateTimeOffset>
		{
			startingDate
		};
		for (int i = 0; i < beginningStarCount - 1; i++)
		{
			var timeSpan = todaysDate - startingDate;
			var randomSpan = new TimeSpan(0, random.Next(0, (int)timeSpan.TotalMinutes), 0);

			var randomDateTimeOffset = startingDate + randomSpan;
			beginningStarredAtList.Add(randomDateTimeOffset);
		}

		Assert.Multiple(() =>
		{
			Assert.That(beginningStarredAtList, Has.Count.EqualTo(beginningStarCount));
			Assert.That(beginningStarredAtList.Min(), Is.EqualTo(startingDate));
			Assert.That(beginningStarredAtList.Max(), Is.LessThanOrEqualTo(todaysDate));
		});

		var repository = CreateRepository(false) with
		{
			StarredAt = beginningStarredAtList
		};

		// Act
		var estimatedStarredAtList = DateTimeService.GetEstimatedStarredAtList(repository, totalStarCount);

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(estimatedStarredAtList, Has.Count.EqualTo(totalStarCount));

			Assert.That(estimatedStarredAtList.Min(), Is.EqualTo(startingDate));
			Assert.That(estimatedStarredAtList[0], Is.EqualTo(estimatedStarredAtList.Min()));

			Assert.That(estimatedStarredAtList.Max(), Is.LessThanOrEqualTo(DateTimeOffset.UtcNow.AddMinutes(10)));
			Assert.That(estimatedStarredAtList[^1], Is.EqualTo(estimatedStarredAtList.Max()));
		});
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

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(estimatedStarredAtList, Has.Count.EqualTo(totalStarCount));

			Assert.That(DateTimeOffset.MinValue, Is.LessThanOrEqualTo(estimatedStarredAtList.Min()));
			Assert.That(estimatedStarredAtList[0], Is.EqualTo(estimatedStarredAtList.Min()));

			Assert.That(estimatedStarredAtList.Max(), Is.LessThanOrEqualTo(DateTimeOffset.UtcNow.AddMinutes(10)));
			Assert.That(estimatedStarredAtList[^1], Is.EqualTo(estimatedStarredAtList.Max()));
		});
	}

	[Test]
	public void GetMinimumDateTimeOffsetTest_NoData()
	{
		// Arrange 
		var expectedDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(13));

		IReadOnlyList<DailyViewsModel> emptyDailyViewsModelList = [];
		IReadOnlyList<DailyClonesModel> emptyDailyClonesModelList = [];

		// Act
		var minimumDailyViewsModel = DateTimeService.GetMinimumDateTimeOffset(emptyDailyViewsModelList);
		var minimumDailyClonesModel = DateTimeService.GetMinimumDateTimeOffset(emptyDailyClonesModelList);

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(minimumDailyViewsModel.Year, Is.EqualTo(expectedDate.Year));
			Assert.That(minimumDailyViewsModel.Month, Is.EqualTo(expectedDate.Month));
			Assert.That(minimumDailyViewsModel.Day, Is.EqualTo(expectedDate.Day));

			Assert.That(minimumDailyClonesModel.Year, Is.EqualTo(expectedDate.Year));
			Assert.That(minimumDailyClonesModel.Month, Is.EqualTo(expectedDate.Month));
			Assert.That(minimumDailyClonesModel.Day, Is.EqualTo(expectedDate.Day));
		});
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

		Assert.Multiple(() =>
		{
			// Assert
			Assert.That(minimumDailyViewsModel.Year, Is.EqualTo(expectedDate.Year));
			Assert.That(minimumDailyViewsModel.Month, Is.EqualTo(expectedDate.Month));
			Assert.That(minimumDailyViewsModel.Day, Is.EqualTo(expectedDate.Day));

			Assert.That(minimumDailyClonesModel.Year, Is.EqualTo(expectedDate.Year));
			Assert.That(minimumDailyClonesModel.Month, Is.EqualTo(expectedDate.Month));
			Assert.That(minimumDailyClonesModel.Day, Is.EqualTo(expectedDate.Day));
		});
	}

	[Test]
	public void GetMaximumDateTimeOffsetTest_NoData()
	{
		// Arrange 
		var expectedDate = DateTimeOffset.UtcNow;

		IReadOnlyList<DailyViewsModel> emptyDailyViewsModelList = [];
		IReadOnlyList<DailyClonesModel> emptyDailyClonesModelList = [];

		// Act
		var minimumDailyViewsModel = DateTimeService.GetMaximumDateTimeOffset(emptyDailyViewsModelList);
		var minimumDailyClonesModel = DateTimeService.GetMaximumDateTimeOffset(emptyDailyClonesModelList);

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(minimumDailyViewsModel.Year, Is.EqualTo(expectedDate.Year));
			Assert.That(minimumDailyViewsModel.Month, Is.EqualTo(expectedDate.Month));
			Assert.That(minimumDailyViewsModel.Day, Is.EqualTo(expectedDate.Day));

			Assert.That(minimumDailyClonesModel.Year, Is.EqualTo(expectedDate.Year));
			Assert.That(minimumDailyClonesModel.Month, Is.EqualTo(expectedDate.Month));
			Assert.That(minimumDailyClonesModel.Day, Is.EqualTo(expectedDate.Day));
		});
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
		Assert.Multiple(() =>
		{
			Assert.That(minimumDailyViewsModel.Year, Is.EqualTo(expectedDate.Year));
			Assert.That(minimumDailyViewsModel.Month, Is.EqualTo(expectedDate.Month));
			Assert.That(minimumDailyViewsModel.Day, Is.EqualTo(expectedDate.Day));

			Assert.That(minimumDailyClonesModel.Year, Is.EqualTo(expectedDate.Year));
			Assert.That(minimumDailyClonesModel.Day, Is.EqualTo(expectedDate.Day));
			Assert.That(minimumDailyClonesModel.Month, Is.EqualTo(expectedDate.Month));
		});
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
		Assert.Multiple(() =>
		{
			Assert.That(dateTimeOffset_Final.Year, Is.EqualTo(dateTimeOffset_Initial.Year));
			Assert.That(dateTimeOffset_Final.Month, Is.EqualTo(dateTimeOffset_Initial.Month));
			Assert.That(dateTimeOffset_Final.Day, Is.EqualTo(dateTimeOffset_Initial.Day));
			Assert.That(dateTimeOffset_Final.Hour, Is.EqualTo(0));
			Assert.That(dateTimeOffset_Final.Minute, Is.EqualTo(0));
			Assert.That(dateTimeOffset_Final.Second, Is.EqualTo(0));
			Assert.That(dateTimeOffset_Final.Millisecond, Is.EqualTo(0));
		});
	}
}