using System.Collections.Generic;

namespace GitTrends.Mobile.Common
{
    public class TrendsChartConstants
    {
        public const string TotalViewsTitle = "Views";
        public const string UniqueViewsTitle = "Unique Views";
        public const string TotalClonesTitle = "Clones";
        public const string UniqueClonesTitle = "Unique Clones";

        public static Dictionary<TrendsChartOption, string> TrendsChartTitles { get; } = new Dictionary<TrendsChartOption, string>
        {
            { TrendsChartOption.All, "All" },
            { TrendsChartOption.NoUniques, "No Uniques" },
            { TrendsChartOption.JustUniques, "Just Uniques" }
        };
    }

    public enum TrendsChartOption  { All, NoUniques, JustUniques }
}
