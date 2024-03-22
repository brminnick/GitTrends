using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GitTrends.Shared;

public static class StringExtensions
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

	public static string RemoveEmoji(this string text) => Regex.Replace(text, @"\p{Cs}", "");
}