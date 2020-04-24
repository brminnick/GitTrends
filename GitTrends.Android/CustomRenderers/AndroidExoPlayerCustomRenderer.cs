using Android.Content;
using Android.Net;
using Android.OS;
using Android.Views.Animations;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using GitTrends;
using GitTrends.Droid;
using Java.IO;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(AndroidExoPlayerView), typeof(VideoPlayerRenderer))]
namespace GitTrends.Droid
{
    public class VideoPlayerRenderer : ViewRenderer<AndroidExoPlayerView, SimpleExoPlayerView>, IAdaptiveMediaSourceEventListener
    {
        SimpleExoPlayerView? _playerView;
        SimpleExoPlayer? _player;

        public VideoPlayerRenderer(Context context) : base(context)
        {
        }

        public void OnDownstreamFormatChanged(int p0, Format p1, int p2, Object p3, long p4)
        {
        }

        public void OnLoadCanceled(DataSpec p0, int p1, int p2, Format p3, int p4, Object p5, long p6, long p7, long p8, long p9, long p10)
        {
        }

        public void OnLoadCompleted(DataSpec p0, int p1, int p2, Format p3, int p4, Object p5, long p6, long p7, long p8, long p9, long p10)
        {
        }

        public void OnLoadError(DataSpec p0, int p1, int p2, Format p3, int p4, Object p5, long p6, long p7, long p8, long p9, long p10, IOException p11, bool p12)
        {
        }

        public void OnLoadStarted(DataSpec p0, int p1, int p2, Format p3, int p4, Object p5, long p6, long p7, long p8)
        {
        }

        public void OnUpstreamDiscarded(int p0, long p1, long p2)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<AndroidExoPlayerView> e)
        {
            base.OnElementChanged(e);

            if (_player is null)
                InitializePlayer();

            if (e.NewElement.SourceUrl != null)
                Play(e.NewElement.SourceUrl);
        }

        void InitializePlayer()
        {
            _player = ExoPlayerFactory.NewSimpleInstance(Context, new DefaultTrackSelector());
            _player.PlayWhenReady = true;
            _player.RepeatMode = (int)RepeatMode.Restart;

            _playerView = new SimpleExoPlayerView(Context)
            {
                UseController = false,
                Player = _player,
                ControllerAutoShow = false
            };

            SetNativeControl(_playerView);
        }

        void Play(string url)
        {
            var sourceUri = Uri.Parse(url);

            var httpDataSourceFactory = new DefaultHttpDataSourceFactory("1");
            var ssChunkFactory = new DefaultSsChunkSource.Factory(httpDataSourceFactory);

            var ssMediaSource = new SsMediaSource(sourceUri, httpDataSourceFactory, ssChunkFactory, new Handler(), this);
            _player?.Prepare(ssMediaSource);
        }
    }
}