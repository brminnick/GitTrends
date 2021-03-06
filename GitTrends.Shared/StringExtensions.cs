using System.Globalization;

namespace GitTrends.Shared
{
    public static class StringExtensions
    {
        public static string ToPascalCase(this string input)
        {
            var resultBuilder = new System.Text.StringBuilder();
            foreach (char c in input)
            {
                if (!char.IsLetterOrDigit(c))
                    resultBuilder.Append(" ");
                else
                    resultBuilder.Append(c);
            }

            string result = resultBuilder.ToString();
            result = result.ToLower();

            var textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(result);
        }
    }
}
