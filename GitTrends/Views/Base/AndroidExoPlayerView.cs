using Xamarin.Forms;

namespace GitTrends
{
    public class AndroidExoPlayerView : View
    {
        public static readonly BindableProperty SourceUrlProperty =
            BindableProperty.Create(nameof(SourceUrl), typeof(string), typeof(AndroidExoPlayerView), null);

        public string? SourceUrl
        {
            get => (string?)GetValue(SourceUrlProperty);
            set => SetValue(SourceUrlProperty, value);
        }
    }
}
