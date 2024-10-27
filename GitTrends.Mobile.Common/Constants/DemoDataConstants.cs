namespace GitTrends.Mobile.Common;

public static class DemoDataConstants
{
	public const int RepoCount = 50;
	public const int ReferringSitesCount = 10;

	public const int MaximumRandomNumber = 100;

	const string _loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum ";

	static readonly Random _random = Random.Shared;

	public static int GetRandomNumber(int minimum = 0) => _random.Next(minimum, MaximumRandomNumber);

	public static string GetRandomText(int? length = null)
	{
		var startIndex = _random.Next(_loremIpsum.Length / 2);

		var maximumLength = _loremIpsum.Length - 1 - startIndex;

		length ??= _random.Next(maximumLength);

		return _loremIpsum.Substring(startIndex, length.Value);
	}

	//https://stackoverflow.com/a/194870/5953643
	public static DateTimeOffset GetRandomDate()
	{
		var gitHubFoundedDate = new DateTimeOffset(2008, 2, 8, 0, 0, 0, TimeSpan.Zero);

		int range = (DateTime.Today - gitHubFoundedDate).Days;

		return gitHubFoundedDate.AddDays(_random.Next(range));
	}

	public static IEnumerable<DateTimeOffset> GenerateStarredAtDates(in int starCount)
	{
		var starGazerList = new List<DateTimeOffset>();

		for (int i = 0; i < starCount; i++)
			starGazerList.Add(GetRandomDate());

		return starGazerList.OrderBy(static x => x);
	}
}