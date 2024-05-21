using GitTrends.Mobile.Common.Constants;

namespace GitTrends.Mobile.Common;

public class ThemePickerConstants
{
	//Keep as expression-bodied member (e.g. don't use a readonly property) to ensure the correct RESX file is uses when the language changes 
	public static IReadOnlyDictionary<PreferredTheme, string> ThemePickerTitles => new Dictionary<PreferredTheme, string>()
	{
		{ PreferredTheme.Default, ThemeConstants.Default },
		{ PreferredTheme.Light, ThemeConstants.Light },
		{ PreferredTheme.Dark, ThemeConstants.Dark }
	};
}