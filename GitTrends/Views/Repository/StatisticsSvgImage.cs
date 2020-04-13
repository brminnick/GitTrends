using Xamarin.Forms;

namespace GitTrends
{
    class StatisticsSvgImage : SvgImage
    {
        public StatisticsSvgImage(in string svgFileName, string baseThemeColor, in double widthRequest = 24, in double heightRequest = 24)
            : base(svgFileName, () => (Color)Application.Current.Resources[baseThemeColor], widthRequest, heightRequest)
        {
            VerticalOptions = LayoutOptions.CenterAndExpand;
            HorizontalOptions = LayoutOptions.EndAndExpand;
        }
    }
}
