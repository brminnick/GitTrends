using System;
using Xamarin.Forms;

namespace GitTrends
{
    //On iOS, use custom renderer for MediaElement until MediaElement.Dispose bug is fixed: https://github.com/xamarin/Xamarin.Forms/issues/9525#issuecomment-619156536
    //On Android, use Custom Renderer for ExoPlayer because Xamarin.Forms.MediaElement uses Android.VideoView
    public class VideoPlayerView : View
    {
        public readonly BindableProperty UriProperty = BindableProperty.Create(nameof(Uri), typeof(Uri), typeof(VideoPlayerView));

        public VideoPlayerView() => BackgroundColor = Color.Transparent;

        public Uri? Uri
        {
            get => (Uri?)GetValue(UriProperty);
            set => SetValue(UriProperty, value);
        }
    }
}
