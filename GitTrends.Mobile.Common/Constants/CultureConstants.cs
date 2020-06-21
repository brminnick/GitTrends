using System.Collections.Generic;

namespace GitTrends.Mobile.Common
{ 
    public static class CultureConstants
    {
        public static Dictionary<string, string> CulturePickerOptions = new Dictionary<string, string>
        {
            {"", "Default" },
            {"de", "🇩🇪 Deutsch" },
            {"en", "🇺🇸 English" },
            {"es", "🇪🇸 Español" },
            {"nl", "🇳🇱 Nederlands" },
            {"ru", "🇷🇺 русский" },
        };
    }
}