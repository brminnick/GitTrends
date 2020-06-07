namespace GitTrends.Mobile.Shared
{
    public static class StatisticsService
    {
        public static string ConvertToAbbreviatedText(this long number)
        {
            if (number < 10e2)
                return string.Format("{0:0}", number);
            else if (number < 10e5)
                return $"{string.Format("{0:0.0}", number / 10e2)}K";
            else if (number < 10e8)
                return $"{string.Format("{0:0.0}", number / 10e5)}M";
            else if (number < 10e11)
                return $"{string.Format("{0:0.0}", number / 10e8)}B";
            else if (number < 10e14)
                return $"{string.Format("{0:0.0}", number / 10e11)}T";

            return "0";
        }
    }
}
