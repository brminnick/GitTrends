using Xamarin.Forms;

namespace GitTrends
{
    public class FontAwesomeButton : Button
    {
        const string _fontAwesomeTypeface = "Font Awesome 5 Brands";

        public FontAwesomeButton() => FontFamily = _fontAwesomeTypeface;
    }
}
