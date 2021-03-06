using System.Collections.Generic;

namespace GitTrends.Mobile.Common
{
    public static class CultureConstants
    {
        public static Dictionary<string, string> CulturePickerOptions { get; } = new Dictionary<string, string>
        {
            {"", "Default" },
            {"bs", "🇧🇦 Bosanski" },
            {"de", "🇩🇪 Deutsch" },
            {"en", "🇺🇸 English" },
            {"es", "🇪🇸 Español" },
            {"fr", "🇫🇷 Français" },
            {"nb", "🇳🇴 Norsk (bokmål)" },
            {"nl", "🇳🇱 Nederlands" },
            {"pt", "🇵🇹 Português" },
            {"ru", "🇷🇺 русский" },
            {"uk", "🇺🇦 Українська" },
            {"tr", "🇹🇷 Türkçe" }
        };
    }
}