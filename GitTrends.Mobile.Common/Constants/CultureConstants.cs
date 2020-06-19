using System.Collections.Generic;

namespace GitTrends.Mobile.Common
{ 
    public static class CultureConstants
    {
        public static Dictionary<string, string> CulturePickerOptions = new Dictionary<string, string>
        {
            {"", "Default" },
            {"en", "🇺🇸 English" },
            {"ru", "🇷🇺 русский" },
            {"de", "🇩🇪 Deutsch" },
            {"nl", "🇳🇱 Nederlands" },
        };
    }
}