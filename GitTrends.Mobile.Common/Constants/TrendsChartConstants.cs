using System.Collections.Generic;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common
{
    public class TrendsChartConstants
    {
        public static Dictionary<TrendsChartOption, string> TrendsChartTitles { get; } = new Dictionary<TrendsChartOption, string>
        {
            { TrendsChartOption.All, TrendsChartTitleConstants.All },
            { TrendsChartOption.NoUniques, TrendsChartTitleConstants.NoUniques },
            { TrendsChartOption.JustUniques, TrendsChartTitleConstants.JustUniques }
        };
    }
}
