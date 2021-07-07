using System.ComponentModel;
using Android.Content;
using Android.Net;
using Android.Views.Animations;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using GitTrends;
using GitTrends.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(VideoPlayerView), typeof(VideoPlayerViewCustomRenderer))]
namespace GitTrends.Droid
{
    public class VideoPlayerViewCustomRenderer : ViewRenderer<VideoPlayerView, PlayerView>
    {
        readonly PlayerView _playerView;
        readonly SimpleExoPlayer _player;

        public VideoPlayerViewCustomRenderer(Context context) : base(context)
        {
            _player = new SimpleExoPlayer.Builder(context).Build();

            _player.PlayWhenReady = true;
            _player.RepeatMode = (int)RepeatMode.Restart;

            _playerView = new PlayerView(context)
            {
                Player = _player,
                UseController = false,
                ControllerAutoShow = false
            };
        }

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public VideoPlayerViewCustomRenderer(System.IntPtr ptr, Android.Runtime.JniHandleOwnership jni)
        {
            //Fixes no constructor found exception
        }
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayerView> e)
        {
            base.OnElementChanged(e);

            if (Control is null)
                SetNativeControl(_playerView);

            if (Element.Url is not null
                && Uri.Parse(Element.Url) is Uri uri)
            {
                Play(uri);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName is nameof(Element.Url)
                && Element.Url is not null
                && Uri.Parse(Element.Url) is Uri uri)
            {
                Play(uri);
            }
        }

        void Play(in Uri uri)
        {
            var httpDataSourceFactory = new DefaultHttpDataSourceFactory(nameof(GitTrends));
            var ssChunkFactory = new DefaultSsChunkSource.Factory(httpDataSourceFactory);

            var ssMediaSourceFactory = new SsMediaSource.Factory(ssChunkFactory, httpDataSourceFactory);

            _player.Prepare(ssMediaSourceFactory.CreateMediaSource(uri));
        }
    }
}