using System.Collections.Generic;
using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common
{
    public class TrendsChartConstants
    {
        //Keep as expression-bodied member (e.g. don't use a readonly property) to ensure the correct RESX file is uses when the language changes 
        public static Dictionary<TrendsChartOption, string> TrendsChartTitles => new Dictionary<TrendsChartOption, string>
        {
            { TrendsChartOption.All, TrendsChartTitleConstants.All },
            { TrendsChartOption.NoUniques, TrendsChartTitleConstants.NoUniques },
            { TrendsChartOption.JustUniques, TrendsChartTitleConstants.JustUniques }
        };
    }
}
