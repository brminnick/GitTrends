using System.Collections.Generic;

namespace GitTrends.Mobile.Shared
{
    class TrendsChartConstants
    {
        public static Dictionary<TrendsChartOption, string> TrendsChartTitles { get; } = new Dictionary<TrendsChartOption, string>
        {
            { TrendsChartOption.All, "All" },
            { TrendsChartOption.NoUniques, "No Uniques" },
            { TrendsChartOption.JustUniques, "Just Uniques" }
        };
    }

    public enum TrendsChartOption  { All, NoUniques, JustUniques }
}
