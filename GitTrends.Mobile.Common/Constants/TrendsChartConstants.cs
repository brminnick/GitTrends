using System.Collections.Generic;

namespace GitTrends.Mobile.Common
{
    public class TrendsChartConstants
    {
        public static Dictionary<TrendsChartOption, string> TrendsChartTitles { get; } = new Dictionary<TrendsChartOption, string>
        {
            { TrendsChartOption.All, "All" },
            { TrendsChartOption.NoUniques, "No Uniques" },
            { TrendsChartOption.JustUniques, "Just Uniques" }
        };
    }
}
