using Xamarin.Essentials;

namespace GitTrends
{
    public static class FirstRunService
    {
        public static bool IsFirstRun
        {
            get => Preferences.Get(nameof(IsFirstRun), true);
            set => Preferences.Set(nameof(IsFirstRun), value);
        }
    }
}
