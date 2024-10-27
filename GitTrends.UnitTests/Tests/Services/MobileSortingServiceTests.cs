using System.ComponentModel;
using GitTrends.Common;
using GitTrends.Mobile.Common;

namespace GitTrends.UnitTests;

class MobileSortingServiceTests : BaseTest
{
	[Test]
	public void SortReferringSitesTests()
	{
		const int largestTotalCount = 1000;
		const int largestTotalUniqueCount = 11;
		const string lastReferer = "t.co";

		//Arrange
		IReadOnlyList<MobileReferringSiteModel> referringSitesList =
		[
			new MobileReferringSiteModel(new ReferringSiteModel(10, 10, "Google")),
			new MobileReferringSiteModel(new ReferringSiteModel(10, 10, "codetraveler.io")),
			new MobileReferringSiteModel(new ReferringSiteModel(10, 10, lastReferer)),
			new MobileReferringSiteModel(new ReferringSiteModel(100, largestTotalUniqueCount, "facebook.com")),
			new MobileReferringSiteModel(new ReferringSiteModel(100, 9, "linkedin.com")),
			new MobileReferringSiteModel(new ReferringSiteModel(largestTotalCount, 9, "reddit.com"))
		];

		//Act
		var sortedReferringSitesList = MobileSortingService.SortReferringSites(referringSitesList).ToList();

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(sortedReferringSitesList[0].TotalCount, Is.EqualTo(largestTotalCount));
			Assert.That(sortedReferringSitesList.Skip(1).First().TotalUniqueCount, Is.EqualTo(largestTotalUniqueCount));
			Assert.That(sortedReferringSitesList.Last().Referrer, Is.EqualTo(lastReferer));
		});
	}

	[TestCase(SortingOption.Clones, true)]
	[TestCase(SortingOption.Forks, true)]
	[TestCase(SortingOption.Issues, true)]
	[TestCase(SortingOption.Stars, true)]
	[TestCase(SortingOption.UniqueClones, true)]
	[TestCase(SortingOption.UniqueViews, true)]
	[TestCase(SortingOption.Views, true)]
	[TestCase(SortingOption.Clones, false)]
	[TestCase(SortingOption.Forks, false)]
	[TestCase(SortingOption.Issues, false)]
	[TestCase(SortingOption.Stars, false)]
	[TestCase(SortingOption.UniqueClones, false)]
	[TestCase(SortingOption.UniqueViews, false)]
	[TestCase(SortingOption.Views, false)]
	public void SortRepositoriesTests(SortingOption sortingOption, bool isReversed)
	{
		//Assert
		Assert.That(MobileSortingService.DefaultSortingOption, Is.EqualTo(SortingOption.Views));

		//Arrange
		Repository topRepository, bottomRepository;

		List<Repository> repositoryList = [];
		for (int i = 0; i < DemoDataConstants.RepoCount; i++)
		{
			repositoryList.Add(CreateRepository());
		}

		//Act
		var sortedRepositoryList = MobileSortingService.SortRepositories(repositoryList, sortingOption, isReversed);
		topRepository = sortedRepositoryList.First();
		bottomRepository = sortedRepositoryList.Last();

		//Assert
		switch (sortingOption)
		{
			case SortingOption.Clones when isReversed:
				ArgumentNullException.ThrowIfNull(bottomRepository.TotalClones);
				Assert.That(topRepository.TotalClones, Is.LessThan(bottomRepository.TotalClones));
				break;
			case SortingOption.Clones:
				ArgumentNullException.ThrowIfNull(bottomRepository.TotalClones);
				Assert.That(topRepository.TotalClones, Is.GreaterThan(bottomRepository.TotalClones));
				break;
			case SortingOption.Forks when isReversed:
				Assert.That(topRepository.ForkCount, Is.LessThan(bottomRepository.ForkCount));
				break;
			case SortingOption.Forks:
				Assert.That(topRepository.ForkCount, Is.GreaterThan(bottomRepository.ForkCount));
				break;
			case SortingOption.Issues when isReversed:
				Assert.That(topRepository.IssuesCount, Is.LessThan(bottomRepository.IssuesCount));
				break;
			case SortingOption.Issues:
				Assert.That(topRepository.IssuesCount, Is.GreaterThan(bottomRepository.IssuesCount));
				break;
			case SortingOption.Stars when isReversed:
				Assert.That(topRepository.StarCount, Is.LessThan(bottomRepository.StarCount));
				break;
			case SortingOption.Stars:
				Assert.That(topRepository.StarCount, Is.GreaterThan(bottomRepository.StarCount));
				break;
			case SortingOption.UniqueClones when isReversed:
				ArgumentNullException.ThrowIfNull(bottomRepository.TotalUniqueClones);
				Assert.That(topRepository.TotalUniqueClones, Is.LessThan(bottomRepository.TotalUniqueClones));
				break;
			case SortingOption.UniqueClones:
				ArgumentNullException.ThrowIfNull(bottomRepository.TotalUniqueClones);
				Assert.That(topRepository.TotalUniqueClones, Is.GreaterThan(bottomRepository.TotalUniqueClones));
				break;
			case SortingOption.UniqueViews when isReversed:
				ArgumentNullException.ThrowIfNull(bottomRepository.TotalUniqueViews);
				Assert.That(topRepository.TotalUniqueViews, Is.LessThan(bottomRepository.TotalUniqueViews));
				break;
			case SortingOption.UniqueViews:
				ArgumentNullException.ThrowIfNull(bottomRepository.TotalUniqueViews);
				Assert.That(topRepository.TotalUniqueViews, Is.GreaterThan(bottomRepository.TotalUniqueViews));
				break;
			case SortingOption.Views when isReversed:
				ArgumentNullException.ThrowIfNull(bottomRepository.TotalViews);
				Assert.That(topRepository.TotalViews, Is.LessThan(bottomRepository.TotalViews));
				break;
			case SortingOption.Views:
				ArgumentNullException.ThrowIfNull(bottomRepository.TotalViews);
				Assert.That(topRepository.TotalViews, Is.GreaterThan(bottomRepository.TotalViews));
				break;
			default:
				throw new NotSupportedException();
		}
	}

	[Test]
	public void IsReversedTest()
	{
		//Arrange
		bool isReversed_Initial, isReversed_AfterTrue, isReversed_AfterFalse;
		var sortingService = ServiceCollection.ServiceProvider.GetRequiredService<MobileSortingService>();

		//Act
		isReversed_Initial = sortingService.IsReversed;

		sortingService.IsReversed = true;
		isReversed_AfterTrue = sortingService.IsReversed;

		sortingService.IsReversed = false;
		isReversed_AfterFalse = sortingService.IsReversed;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(isReversed_Initial, Is.False);
			Assert.That(isReversed_AfterTrue);
			Assert.That(isReversed_AfterFalse, Is.False);
		});
	}

	[TestCase(SortingOption.Clones, SortingCategory.Clones)]
	[TestCase(SortingOption.Forks, SortingCategory.IssuesForks)]
	[TestCase(SortingOption.Issues, SortingCategory.IssuesForks)]
	[TestCase(SortingOption.Stars, SortingCategory.Views)]
	[TestCase(SortingOption.UniqueClones, SortingCategory.Clones)]
	[TestCase(SortingOption.UniqueViews, SortingCategory.Views)]
	[TestCase(SortingOption.Views, SortingCategory.Views)]
	public void GetSortingCategoryTest_ValidOption(SortingOption sortingOption, SortingCategory expectedSortingCategory)
	{
		//Arrange
		SortingCategory actualSortingCategory;

		//Act
		actualSortingCategory = MobileSortingService.GetSortingCategory(sortingOption);

		//Assert
		Assert.That(actualSortingCategory, Is.EqualTo(expectedSortingCategory));
	}

	[TestCase(int.MinValue)]
	[TestCase(-1)]
	[TestCase(7)]
	[TestCase(int.MaxValue)]
	public void GetSortingCategoryTest_InvalidOption(SortingOption sortingOption)
	{
		//Arrange

		//Act //Assert
		Assert.Throws<NotSupportedException>(() => MobileSortingService.GetSortingCategory(sortingOption));
	}

	[TestCase(SortingOption.Clones)]
	[TestCase(SortingOption.Forks)]
	[TestCase(SortingOption.Issues)]
	[TestCase(SortingOption.Stars)]
	[TestCase(SortingOption.UniqueClones)]
	[TestCase(SortingOption.UniqueViews)]
	[TestCase(SortingOption.Views)]
	public void CurrentOptionTest_ValidOption(SortingOption sortingOption)
	{
		//Arrange
		SortingOption currentOption_Initial, currentOption_Final;

		var sortingService = ServiceCollection.ServiceProvider.GetRequiredService<MobileSortingService>();

		//Act
		currentOption_Initial = sortingService.CurrentOption;

		sortingService.CurrentOption = sortingOption;
		currentOption_Final = sortingService.CurrentOption;

		//Assert
		Assert.Multiple(() =>
		{
			Assert.That(currentOption_Initial, Is.EqualTo(MobileSortingService.DefaultSortingOption));
			Assert.That(currentOption_Final, Is.EqualTo(sortingOption));
		});
	}

	[Test]
	public void CurrentOptionTest_InvalidOption()
	{
		//Arrange
		SortingOption currentOption_Initial;

		var sortingService = ServiceCollection.ServiceProvider.GetRequiredService<MobileSortingService>();

		//Act
		currentOption_Initial = sortingService.CurrentOption;

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(currentOption_Initial, Is.EqualTo(MobileSortingService.DefaultSortingOption));

			Assert.Throws<InvalidEnumArgumentException>(() =>
			{
				sortingService.CurrentOption = (SortingOption)(Enum.GetNames(typeof(SortingOption)).Count() + 1);
			});

			Assert.Throws<InvalidEnumArgumentException>(() =>
			{
				sortingService.CurrentOption = (SortingOption)(-1);
			});
		});
	}
}