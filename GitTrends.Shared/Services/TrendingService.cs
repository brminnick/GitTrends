using System;
using System.Collections.Generic;
using System.Linq;

namespace GitTrends.Shared
{
    static class TrendingService
    {
        public static (bool? isViewsTrending, bool? isClonesTrending) IsTrending(in Repository repository)
        {
            var sortedDailyViewsList = repository.DailyViewsList.OrderBy(x => x.TotalViews).ToList();
            var sortedDailyClonesList = repository.DailyClonesList.OrderBy(x => x.TotalClones).ToList();

            return (DoesContainUpperOutlier(sortedDailyViewsList),
                        DoesContainUpperOutlier(sortedDailyClonesList));
        }

        public static bool? IsTrending(in List<DailyClonesModel> dailyClones)
        {
            var sortedDailyClonesList = dailyClones.OrderByDescending(x => x.TotalClones).ToList();

            return DoesContainUpperOutlier(sortedDailyClonesList);
        }

        public static bool? IsTrending(in List<DailyViewsModel> dailyViews)
        {
            var sortedDailyViewsList = dailyViews.OrderByDescending(x => x.TotalViews).ToList();

            return DoesContainUpperOutlier(sortedDailyViewsList);
        }

        static bool? DoesContainUpperOutlier(in List<DailyViewsModel> dailyViewsModels)
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

        static bool? DoesContainUpperOutlier(in List<DailyClonesModel> dailyClonesModels)
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

        static (int? Q1, int? Q2, int? Q3) GetQuartileIndicies<T>(in List<T> list)
        {
            if (list.Count > 1)
                return (list.Count / 4, list.Count / 2, list.Count * 3 / 4);
            else
                return (null, null, null);
        }

        static IEnumerable<T> GetTwoMostRecentDays<T>(in List<T> dailyClonesModels) where T : BaseDailyModel
        {
            return dailyClonesModels.Where(x => DateTime.Compare(x.LocalDay.AddDays(2), DateTime.Now) > 0);
        }
    }
}
