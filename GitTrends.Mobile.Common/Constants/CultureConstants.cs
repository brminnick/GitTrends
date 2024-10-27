using GitTrends.Common;

namespace GitTrends.Mobile.Common;

public static class CultureConstants
{
	static readonly IReadOnlyDictionary<string, string> _cultureOptions = new Dictionary<string, string>
	{
		{ "bs", "🇧🇦 Bosanski" },
		{ "cs", "🇨🇿 Čeština" },
		{ "de", "🇩🇪 Deutsch" },
		{ "en", "🇺🇸 English" },
		{ "es", "🇪🇸 Español" },
		{ "fr", "🇫🇷 Français" },
		{ "nb", "🇳🇴 Norsk (bokmål)" },
		{ "nl", "🇳🇱 Nederlands" },
		{ "pt", "🇵🇹 Português" },
		{ "ru", "🇷🇺 русский" },
		{ "uk", "🇺🇦 Українська" },
		{ "tr", "🇹🇷 Türkçe" }
	};

	public static IReadOnlyDictionary<string, string> CulturePickerOptions { get; } = InitializeCulturePickerOptions();

	static IReadOnlyDictionary<string, string> InitializeCulturePickerOptions()
	{
		var culturePickerOptions = new Dictionary<string, string>
		{
			{"", "Default" }
		};

		foreach (var keyValuePair in _cultureOptions.OrderBy(static x => x.Value.RemoveEmoji()))
			culturePickerOptions.Add(keyValuePair.Key, keyValuePair.Value);

		return culturePickerOptions;
	}
}