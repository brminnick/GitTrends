using System.Globalization;
using System.Text.RegularExpressions;

namespace  GitTrends.Common;

public static partial class StringExtensions
{
	public static string ToPascalCase(this string input)
	{
		var resultBuilder = new System.Text.StringBuilder();
		foreach (char c in input)
		{
			if (!char.IsLetterOrDigit(c))
				resultBuilder.Append(' ');
			else
				resultBuilder.Append(c);
		}

		var result = resultBuilder.ToString().ToLower();

		var textInfo = new CultureInfo("en-US", false).TextInfo;
		return textInfo.ToTitleCase(result);
	}

	public static string RemoveEmoji(this string text) => RemoveEmojiRegEx().Replace(text, "");
	
	[GeneratedRegex("\\p{Cs}")]
	private static partial Regex RemoveEmojiRegEx();
}