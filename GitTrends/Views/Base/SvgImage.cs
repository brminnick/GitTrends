using System;
using FFImageLoading.Svg.Forms;
using Xamarin.Forms;

namespace GitTrends
{
    class SvgImage : SvgCachedImage
    {
        readonly Func<Color> _textColor;

        public SvgImage(in string svgFileName, in Func<Color> textColor)
        {
            _textColor = textColor;

            var app = (App)Application.Current;
            app.ThemeChanged += HandleThemeChanged;

            UpdateSVGColor();

            Source = SvgService.GetFullPath(svgFileName);
        }

        void HandleThemeChanged(object sender, Theme e) => UpdateSVGColor();

        void UpdateSVGColor() => ReplaceStringMap = SvgService.GetColorStringMap(_textColor());
    }
}
