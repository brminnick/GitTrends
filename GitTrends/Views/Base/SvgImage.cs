using System;
using FFImageLoading.Svg.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    public class SvgImage : SvgCachedImage
    {
        readonly Func<Color> _getTextColor;

        public SvgImage(in string svgFileName, in Func<Color> getTextColor, double widthRequest = 24, double heightRequest = 24)
        {
            if (!svgFileName.EndsWith(".svg"))
                throw new ArgumentException($"{nameof(svgFileName)} must end with .svg", nameof(svgFileName));

            Application.Current.RequestedThemeChanged += HandleRequestedThemeChanged;

            this.FillExpand();

            _getTextColor = getTextColor;

            UpdateSVGColor();

            Source = SvgService.GetFullPath(svgFileName);

            WidthRequest = widthRequest;
            HeightRequest = heightRequest;
        }

        void HandleRequestedThemeChanged(object sender, AppThemeChangedEventArgs e) => UpdateSVGColor();

        void UpdateSVGColor() => ReplaceStringMap = SvgService.GetColorStringMap(_getTextColor());
    }
}
