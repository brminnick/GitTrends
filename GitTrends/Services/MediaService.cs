using Xamarin.Forms;

namespace GitTrends
{
    static class MediaService
    {
        public static MediaSource GetMediaSource(in string fileName) => MediaSource.FromFile(fileName);
    }
}
