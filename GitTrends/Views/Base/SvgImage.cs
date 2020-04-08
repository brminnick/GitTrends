using System;
using FFImageLoading.Svg.Forms;
using Xamarin.Forms;

namespace GitTrends
{
    public class SvgImage : SvgCachedImage
    {
        readonly Func<Color> _getTextColor;

        public SvgImage(in string svgFileName, in Func<Color> getTextColor, double widthRequest = 24, double heightRequest = 24)
        {
            if (!svgFileName.EndsWith(".svg"))
                throw new ArgumentException($"{nameof(svgFileName)} must end with .svg", nameof(svgFileName));

            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.FillAndExpand;

            _getTextColor = getTextColor;

            var app = (App)Application.Current;
            app.ThemeChanged += HandleThemeChanged;

            UpdateSVGColor();

            Source = SvgService.GetFullPath(svgFileName);

            WidthRequest = widthRequest;
            HeightRequest = heightRequest;
        }

        void HandleThemeChanged(object sender, Theme e) => UpdateSVGColor();

        void UpdateSVGColor() => ReplaceStringMap = SvgService.GetColorStringMap(_getTextColor());
    }
}
