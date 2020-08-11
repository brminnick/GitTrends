using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class ThemeService
    {
        readonly static WeakEventManager<PreferredTheme> _preferenceChangedEventManager = new WeakEventManager<PreferredTheme>();

        readonly IPreferences _preferences;
        readonly IAnalyticsService _analyticsService;
        readonly IMainThread _mainThread;

        public ThemeService(IAnalyticsService analyticsService, IPreferences preferences, IMainThread mainThread)
        {
            _mainThread = mainThread;
            _preferences = preferences;
            _analyticsService = analyticsService;
        }

        public static event EventHandler<PreferredTheme> PreferenceChanged
        {
            add => _preferenceChangedEventManager.AddEventHandler(value);
            remove => _preferenceChangedEventManager.RemoveEventHandler(value);
        }

        public PreferredTheme Preference
        {
            get => (PreferredTheme)_preferences.Get(nameof(Preference), (int)PreferredTheme.Default);
            set
            {
                _preferences.Set(nameof(Preference), (int)value);
                SetAppTheme(value).SafeFireAndForget();
            }
        }

        internal Task Initialize()
        {
            if (Application.Current != null)
                Application.Current.RequestedThemeChanged += HandleRequestedThemeChanged;

            return SetAppTheme(Preference);
        }

        Task SetAppTheme(PreferredTheme preferredTheme)
        {
            if (Application.Current is null)
                return Task.CompletedTask;

            return _mainThread.InvokeOnMainThreadAsync(() =>
            {
                var theme = preferredTheme switch
                {
                    PreferredTheme.Dark => new DarkTheme(),
                    PreferredTheme.Light => new LightTheme(),
                    PreferredTheme.Default => Application.Current.RequestedTheme is OSAppTheme.Dark ? new DarkTheme() : (BaseTheme)new LightTheme(),
                    _ => throw new NotSupportedException()
                };

                if (Application.Current.Resources.GetType() != theme.GetType())
                {
                    Application.Current.Resources = theme;

                    _analyticsService.Track("Theme Changed", new Dictionary<string, string>
                    {
                        { nameof(PreferredTheme), preferredTheme.ToString() },
                        { nameof(Application.Current.RequestedTheme), Application.Current.RequestedTheme.ToString() }
                    });

                    OnPreferenceChanged(preferredTheme);

                    EnableDebugRainbows(false);
                }
            });
        }

        [Conditional("DEBUG")]
        void EnableDebugRainbows(bool shouldUseDebugRainbows)
        {
            Application.Current.Resources.Add(new Style(typeof(ContentPage))
            {
                ApplyToDerivedTypes = true,
                Setters =
                {
                    new Setter
                    {
                        Property = Xamarin.Forms.DebugRainbows.DebugRainbow.ShowColorsProperty,
                        Value = shouldUseDebugRainbows
                    },
                    new Setter
                    {
                        Property = Xamarin.Forms.DebugRainbows.DebugRainbow.ShowGridProperty,
                        Value = shouldUseDebugRainbows
                    }
                }
            });
        }

        async void HandleRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            if (Preference is PreferredTheme.Default)
                await SetAppTheme(PreferredTheme.Default);
        }

        void OnPreferenceChanged(PreferredTheme theme) => _mainThread.InvokeOnMainThreadAsync(() => _preferenceChangedEventManager.RaiseEvent(this, theme, nameof(PreferenceChanged)));
    }
}
