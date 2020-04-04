using System;
using FFImageLoading.Svg.Forms;
using Xamarin.Forms;

namespace GitTrends
{
    class SvgImage : SvgCachedImage
    {
        readonly Func<Color> _getTextColor;

        public SvgImage(in string svgFileName, in Func<Color> getTextColor, double widthRequest = default, double heightRequest = default)
        {
            if (!svgFileName.EndsWith(".svg"))
                throw new ArgumentException($"{nameof(svgFileName)} must end with .svg", nameof(svgFileName));

            _getTextColor = getTextColor;

            var app = (App)Application.Current;
            app.ThemeChanged += HandleThemeChanged;

            UpdateSVGColor();

            Source = SvgService.GetFullPath(svgFileName);

            WidthRequest = (widthRequest == default) ? int.MaxValue : widthRequest;
            HeightRequest = (heightRequest == default) ? int.MaxValue : heightRequest;
        }

        void HandleThemeChanged(object sender, Theme e) => UpdateSVGColor();

        void UpdateSVGColor() => ReplaceStringMap = SvgService.GetColorStringMap(_getTextColor());
    }
}
