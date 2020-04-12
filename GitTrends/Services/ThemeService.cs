using System;
using System.Diagnostics;
using AsyncAwaitBestPractices;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class ThemeService
    {
        readonly static WeakEventManager<PreferredTheme> _preferenceChangedEventManager = new WeakEventManager<PreferredTheme>();

        public ThemeService()
        {
            Application.Current.RequestedThemeChanged += HandleRequestedThemeChanged;
            SetAppTheme(Preference);
        }

        public static event EventHandler<PreferredTheme> PreferenceChanged
        {
            add => _preferenceChangedEventManager.AddEventHandler(value);
            remove => _preferenceChangedEventManager.RemoveEventHandler(value);
        }

        public PreferredTheme Preference
        {
            get => (PreferredTheme)Preferences.Get(nameof(Preference), (int)PreferredTheme.Default);
            set
            {
                Preferences.Set(nameof(Preference), (int)value);
                SetAppTheme(value);
            }
        }

        void SetAppTheme(PreferredTheme preferredTheme)
        {
            BaseTheme theme = preferredTheme switch
            {
                PreferredTheme.Dark => new DarkTheme(),
                PreferredTheme.Light => new LightTheme(),
                PreferredTheme.Default => Application.Current.RequestedTheme is Xamarin.Forms.AppTheme.Dark ? new DarkTheme() : (BaseTheme)new LightTheme(),
                _ => throw new NotSupportedException()
            };

            if (Application.Current.Resources.GetType() != theme.GetType())
            {
                Application.Current.Resources = theme;

                EnableDebugRainbows(false);
            }
        }

        [Conditional("DEBUG")]
        void EnableDebugRainbows(bool shouldUseDebugRainbows)
        {
            Application.Current.Resources.Add(new Style(typeof(ContentPage))
            {
                ApplyToDerivedTypes = true,
                Setters = {
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

        void HandleRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            if (Preference is PreferredTheme.Default)
                SetAppTheme(PreferredTheme.Default);
        }

        void OnPreferenceChanged(PreferredTheme theme) => _preferenceChangedEventManager.HandleEvent(this, theme, nameof(PreferenceChanged));
    }

    public enum PreferredTheme { Default, Light, Dark }
}
