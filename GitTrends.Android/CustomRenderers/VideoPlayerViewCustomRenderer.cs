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
                UseController = false,
                Player = _player,
                ControllerAutoShow = false
            };
        }

        //Fixes no constructor found exception
        public VideoPlayerViewCustomRenderer(System.IntPtr ptr, Android.Runtime.JniHandleOwnership jni)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayerView> e)
        {
            base.OnElementChanged(e);

            if (Control is null)
                SetNativeControl(_playerView);

            Play(MediaElementService.OnboardingChart?.ManifestUrl);
        }

        void Play(string url)
        {
            var httpDataSourceFactory = new DefaultHttpDataSourceFactory(nameof(GitTrends));
            var ssChunkFactory = new DefaultSsChunkSource.Factory(httpDataSourceFactory);

            var ssMediaSourceFactory = new SsMediaSource.Factory(ssChunkFactory, httpDataSourceFactory);

            _player.Prepare(ssMediaSourceFactory.CreateMediaSource(Uri.Parse(url)));
        }
    }
}