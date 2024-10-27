namespace GitTrends.Common;

static class TrendingService
{
	public static (bool? isViewsTrending, bool? isClonesTrending) IsTrending(this Repository repository) =>
		(repository.DailyViewsList.IsTrending(), repository.DailyClonesList.IsTrending());

	public static bool? IsTrending(this IEnumerable<DailyClonesModel>? dailyClones)
	{
		if (dailyClones is null)
			return false;

		var sortedDailyClonesList = dailyClones.OrderBy(static x => x.TotalClones);

		return DoesContainUpperOutlier([.. sortedDailyClonesList]);
	}

	public static bool? IsTrending(this IEnumerable<DailyViewsModel>? dailyViews)
	{
		if (dailyViews is null)
			return false;

		var sortedDailyViewsList = dailyViews.OrderBy(static x => x.TotalViews);

		return DoesContainUpperOutlier([.. sortedDailyViewsList]);
	}

	static bool? DoesContainUpperOutlier(in IList<DailyViewsModel> dailyViewsModels)
	{
		var quartileIndicies = GetQuartileIndicies(dailyViewsModels);

		if (quartileIndicies.Q1 is null || quartileIndicies.Q2 is null || quartileIndicies.Q3 is null)
		{
			return null;
		}

		var quartile1Value = dailyViewsModels[quartileIndicies.Q1.Value].TotalViews;
		var quartile2Value = dailyViewsModels[quartileIndicies.Q2.Value].TotalViews;
		var quartile3Value = dailyViewsModels[quartileIndicies.Q3.Value].TotalViews;

		var interQuartileRange = quartile3Value - quartile1Value;

		var upperOuterFence = quartile3Value + interQuartileRange * 3;

		return upperOuterFence > 10 && GetTwoMostRecentDays(dailyViewsModels).Any(x => x.TotalViews > upperOuterFence);
	}

	static bool? DoesContainUpperOutlier(in IList<DailyClonesModel> dailyClonesModels)
	{
		var quartileIndicies = GetQuartileIndicies(dailyClonesModels);

		if (quartileIndicies.Q1 is null || quartileIndicies.Q2 is null || quartileIndicies.Q3 is null)
		{
			return null;
		}

		var quartile1Value = dailyClonesModels[quartileIndicies.Q1.Value].TotalClones;
		var quartile2Value = dailyClonesModels[quartileIndicies.Q2.Value].TotalClones;
		var quartile3Value = dailyClonesModels[quartileIndicies.Q3.Value].TotalClones;

		var interQuartileRange = quartile3Value - quartile1Value;

		var upperOuterFence = quartile3Value + interQuartileRange * 3;

		return upperOuterFence > 10 && GetTwoMostRecentDays(dailyClonesModels).Any(x => x.TotalClones > upperOuterFence);
	}

	static (int? Q1, int? Q2, int? Q3) GetQuartileIndicies<T>(in IList<T> list)
	{
		if (list.Count > 1)
			return (list.Count / 4, list.Count / 2, list.Count * 3 / 4);
		else
			return (null, null, null);
	}

	static IEnumerable<T> GetTwoMostRecentDays<T>(in IList<T> dailyClonesModels) where T : IBaseDailyModel =>
		dailyClonesModels.Where(static x => x.LocalDay.AddDays(2) > DateTime.Now);
}