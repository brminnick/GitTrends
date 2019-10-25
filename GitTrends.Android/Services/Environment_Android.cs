using System;
using Android.Content.Res;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using GitTrends.Droid;

[assembly: Dependency(typeof(Environment_Android))]
namespace GitTrends.Droid
{
    public class Environment_Android : IEnvironment
    {
        public Theme GetOperatingSystemTheme()
        {
            var uiModeFlags = CrossCurrentActivity.Current.AppContext.Resources.Configuration.UiMode & UiMode.NightMask;

            return uiModeFlags switch
            {
                UiMode.NightYes => Theme.Dark,
                UiMode.NightNo => Theme.Light,
                _ => throw new NotSupportedException($"UiMode {uiModeFlags} not supported"),
            };
        }
    }
}