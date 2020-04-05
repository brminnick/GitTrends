using System;
using Xamarin.Forms;

namespace GitTrends.Views.Base
{
    class RepositoryStatSVGImage : SvgImage
    {
        public RepositoryStatSVGImage(in string svgFileName, string baseThemeColor, in double width = default, in double height = default)
            : base(svgFileName, () => (Color)Application.Current.Resources[baseThemeColor])
        {
            WidthRequest = (width == default) ? int.MaxValue : width;
            HeightRequest = (height == default) ? int.MaxValue : height;

            VerticalOptions = LayoutOptions.CenterAndExpand;
            HorizontalOptions = LayoutOptions.EndAndExpand;
        }
    }
}
