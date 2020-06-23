using System;
using System.Globalization;
using System.Linq;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
    public class LanguageService
    {
        readonly static WeakEventManager<string> _preferredLanguageChangedEventManager = new WeakEventManager<string>();

        readonly IPreferences _preferences;
        readonly IAnalyticsService _analyticsService;
        readonly IMainThread _mainThread;

        public LanguageService(IAnalyticsService analyticsService, IPreferences preferences, IMainThread mainThread)
        {
            _mainThread = mainThread;
            _preferences = preferences;
            _analyticsService = analyticsService;
        }

        public static event EventHandler<string> PreferredLanguageChanged
        {
            add => _preferredLanguageChangedEventManager.AddEventHandler(value);
            remove => _preferredLanguageChangedEventManager.RemoveEventHandler(value);
        }

        public string PreferedLanguage
        {
            get => _preferences.Get(nameof(PreferedLanguage), string.Empty);
            set
            {
                if (!CultureConstants.CulturePickerOptions.Keys.Contains(value))
                    throw new ArgumentException($"{nameof(CultureConstants)}.{nameof(CultureConstants.CulturePickerOptions)} does not contain a key for {value}");

                _preferences.Set(nameof(PreferedLanguage), value);
                SetLanguage(value);
            }
        }

        public void Initialize() => SetLanguage(PreferedLanguage);

        void SetLanguage(in string culture)
        {
            var currentCulture = CultureInfo.DefaultThreadCurrentUICulture?.Name;

            if (currentCulture != culture)
            {
                CultureInfo.DefaultThreadCurrentCulture= new CultureInfo(culture, false);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture, false);

                OnPreferredLanguageChanged(culture);
            }
        }

        void OnPreferredLanguageChanged(string culture) => _preferredLanguageChangedEventManager.HandleEvent(this, culture, nameof(PreferredLanguageChanged));
    }
}
