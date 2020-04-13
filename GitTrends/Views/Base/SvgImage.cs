using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using FFImageLoading.Svg.Forms;
using GitTrends.Mobile.Shared;
using Shiny;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    public class SvgImage : SvgCachedImage
    {
        readonly WeakEventManager<Color> _svgColorChangedEventManager = new WeakEventManager<Color>();

        Func<Color> _getTextColor;

        public SvgImage(in string svgFileName, in Func<Color> getTextColor, double widthRequest = 24, double heightRequest = 24)
        {
            if (!svgFileName.EndsWith(".svg"))
                throw new ArgumentException($"{nameof(svgFileName)} must end with .svg", nameof(svgFileName));

            ThemeService.PreferenceChanged += HandlePreferenceChanged;

            this.FillExpand();

            _getTextColor = getTextColor;

            SetSvgColor();

            Source = SvgService.GetFullPath(svgFileName);

            WidthRequest = widthRequest;
            HeightRequest = heightRequest;
        }

        public event EventHandler<Color> SvgColorChanged
        {
            add => _svgColorChangedEventManager.AddEventHandler(value);
            remove => _svgColorChangedEventManager.RemoveEventHandler(value);
        }

        public async Task UpdateColor(Func<Color> getTextColor)
        {
            _getTextColor = getTextColor;
            await SetSvgColorAsync();
            OnSvgColorChanged(getTextColor());
        }

        void HandlePreferenceChanged(object sender, PreferredTheme e) => SetSvgColor();

        Task SetSvgColorAsync() => MainThread.InvokeOnMainThreadAsync(SetSvgColor);

        void SetSvgColor() => ReplaceStringMap = SvgService.GetColorStringMap(_getTextColor());

        void OnSvgColorChanged(Color color) => _svgColorChangedEventManager.HandleEvent(this, color, nameof(SvgColorChanged));
    }
}
