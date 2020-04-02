using Xamarin.Essentials;

namespace GitTrends
{
    static class FirstRunService
    {
        public static bool IsFirstRun
        {
            get => Preferences.Get(nameof(IsFirstRun), true);
            set => Preferences.Set(nameof(IsFirstRun), value);
        }
    }
}
