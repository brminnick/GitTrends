using Xamarin.Forms;

namespace GitTrends
{
    public class FontAwesomeButton : Button
    {
        public const char GitHubOctocat = '\uf09b';

        const string _typeface = "Font Awesome 5 Brands";

        public FontAwesomeButton() => FontFamily = _typeface;
    }
}
