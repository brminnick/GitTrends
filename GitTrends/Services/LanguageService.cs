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
        readonly static WeakEventManager<string?> _preferredLanguageChangedEventManager = new WeakEventManager<string?>();

        readonly IPreferences _preferences;
        readonly IAnalyticsService _analyticsService;
        readonly IMainThread _mainThread;

        public LanguageService(IAnalyticsService analyticsService, IPreferences preferences, IMainThread mainThread)
        {
            _mainThread = mainThread;
            _preferences = preferences;
            _analyticsService = analyticsService;
        }

        public static event EventHandler<string?> PreferredLanguageChanged
        {
            add => _preferredLanguageChangedEventManager.AddEventHandler(value);
            remove => _preferredLanguageChangedEventManager.RemoveEventHandler(value);
        }

        public string? PreferredLanguage
        {
            get => _preferences.Get(nameof(PreferredLanguage), null);
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = null;
                else if (!CultureConstants.CulturePickerOptions.Keys.Contains(value))
                    throw new ArgumentException($"{nameof(CultureConstants)}.{nameof(CultureConstants.CulturePickerOptions)} does not contain a key for {value ?? "null"}");

                _preferences.Set(nameof(PreferredLanguage), value);
                SetLanguage(value);
            }
        }

        public void Initialize() => SetLanguage(PreferredLanguage);

        void SetLanguage(in string? culture)
        {
            var currentCulture = CultureInfo.DefaultThreadCurrentUICulture?.Name ?? CultureInfo.DefaultThreadCurrentCulture?.Name;

            if (currentCulture != culture)
            {
                CultureInfo.DefaultThreadCurrentCulture = getCultureInfo(culture);
                CultureInfo.DefaultThreadCurrentUICulture = getCultureInfo(culture);

                _analyticsService.Track("Preferred Language Changed", nameof(culture), culture ?? "null");

                OnPreferredLanguageChanged(culture);
            }

            static CultureInfo? getCultureInfo(in string? culture) => culture switch
            {
                null => null,
                _ => new CultureInfo(culture, false)
            };
        }

        void OnPreferredLanguageChanged(in string? culture) => _preferredLanguageChangedEventManager.RaiseEvent(this, culture, nameof(PreferredLanguageChanged));
    }
}
