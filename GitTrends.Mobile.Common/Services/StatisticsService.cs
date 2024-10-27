using GitTrends.Common;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common;

public static class StatisticsService
{
	public static string ToAbbreviatedText(this int number) => ToAbbreviatedText((int?)number);
	public static string ToAbbreviatedText(this int? number) => ToAbbreviatedText((double?)number);

	public static string ToAbbreviatedText(this long number) => ToAbbreviatedText((long?)number);
	public static string ToAbbreviatedText(this long? number) => ToAbbreviatedText((double?)number);

	public static string ToAbbreviatedText(this double number) => ToAbbreviatedText((double?)number);
	public static string ToAbbreviatedText(this double? number) => number switch
	{
		< 10e2 => $"{number:0}",
		< 10e5 => $"{number / 10e2:0.0}K",
		< 10e8 => $"{number / 10e5:0.0}M",
		< 10e11 => $"{number / 10e8:0.0}B",
		< 10e14 => $"{number / 10e11:0.0}T",
		_ => "0"
	};

	public static string GetInformationLabelText<TRepository>(in IReadOnlyList<TRepository> repositories, in MobileSortingService mobileSortingService) where TRepository : IRepository =>
		GetInformationLabelText(repositories, MobileSortingService.GetSortingCategory(mobileSortingService.CurrentOption));

	public static string GetInformationLabelText<TRepository>(in IReadOnlyList<TRepository> repositories, in SortingCategory sortingCategory) where TRepository : IRepository => (repositories.Any(), sortingCategory) switch
	{
		(false, _) => string.Empty,
		(true, SortingCategory.Views) => $"{SortingConstants.Views} {repositories.Sum(static x => x?.TotalViews ?? 0).ToAbbreviatedText()}, {SortingConstants.UniqueViews} {repositories.Sum(static x => x?.TotalUniqueViews ?? 0).ToAbbreviatedText()}, {SortingConstants.Stars} {repositories.Sum(static x => x?.StarCount ?? 0).ToAbbreviatedText()}",
		(true, SortingCategory.Clones) => $"{SortingConstants.Clones} {repositories.Sum(static x => x?.TotalClones ?? 0).ToAbbreviatedText()}, {SortingConstants.UniqueClones} {repositories.Sum(static x => x?.TotalUniqueClones ?? 0).ToAbbreviatedText()}, {SortingConstants.Stars} {repositories.Sum(static x => x?.StarCount ?? 0).ToAbbreviatedText()}",
		(true, SortingCategory.IssuesForks) => $"{SortingConstants.Stars} {repositories.Sum(static x => x?.StarCount ?? 0).ToAbbreviatedText()}, {SortingConstants.Forks} {repositories.Sum(static x => x.ForkCount).ToAbbreviatedText()}, {SortingConstants.Issues} {repositories.Sum(static x => x.IssuesCount).ToAbbreviatedText()}",
		(_, _) => throw new NotSupportedException()
	};

	public static string GetFloatingActionTextButtonText<TRepository>(in MobileSortingService mobileSortingService, in IReadOnlyList<TRepository> repositories, in FloatingActionButtonType floatingActionButtonType) where TRepository : IRepository =>
		GetFloatingActionTextButtonText(MobileSortingService.GetSortingCategory(mobileSortingService.CurrentOption), repositories, floatingActionButtonType);

	public static string GetFloatingActionTextButtonText<TRepository>(in SortingCategory sortingCategory, in IReadOnlyList<TRepository> repositories, in FloatingActionButtonType floatingActionButtonType) where TRepository : IRepository
	{
		return (sortingCategory, floatingActionButtonType) switch
		{
			(SortingCategory.Clones, FloatingActionButtonType.Statistic1) => repositories.Sum(static x => x?.TotalClones ?? 0).ToAbbreviatedText(),
			(SortingCategory.Clones, FloatingActionButtonType.Statistic2) => repositories.Sum(static x => x?.TotalUniqueClones ?? 0).ToAbbreviatedText(),
			(SortingCategory.Clones, FloatingActionButtonType.Statistic3) => repositories.Sum(static x => x?.StarCount ?? 0).ToAbbreviatedText(),
			(SortingCategory.Views, FloatingActionButtonType.Statistic1) => repositories.Sum(static x => x?.TotalViews ?? 0).ToAbbreviatedText(),
			(SortingCategory.Views, FloatingActionButtonType.Statistic2) => repositories.Sum(static x => x?.TotalUniqueViews ?? 0).ToAbbreviatedText(),
			(SortingCategory.Views, FloatingActionButtonType.Statistic3) => repositories.Sum(static x => x?.StarCount ?? 0).ToAbbreviatedText(),
			(SortingCategory.IssuesForks, FloatingActionButtonType.Statistic1) => repositories.Sum(static x => x?.StarCount ?? 0).ToAbbreviatedText(),
			(SortingCategory.IssuesForks, FloatingActionButtonType.Statistic2) => repositories.Sum(static x => x.ForkCount).ToAbbreviatedText(),
			(SortingCategory.IssuesForks, FloatingActionButtonType.Statistic3) => repositories.Sum(static x => x.IssuesCount).ToAbbreviatedText(),
			(_, FloatingActionButtonType.Information) => throw new NotSupportedException(),
			(_, _) => throw new NotImplementedException()
		};
	}
}