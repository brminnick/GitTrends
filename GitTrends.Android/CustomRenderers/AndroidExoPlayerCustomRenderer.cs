using System.ComponentModel;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Views.Animations;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using GitTrends;
using GitTrends.Droid;
using Java.IO;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(AndroidExoPlayerView), typeof(AndroidExoPlayerCustomRenderer))]
namespace GitTrends.Droid
{
    public class AndroidExoPlayerCustomRenderer : ViewRenderer<AndroidExoPlayerView, PlayerView>, IMediaSourceEventListener
    {
        readonly PlayerView _playerView;
        readonly SimpleExoPlayer _player;

        public AndroidExoPlayerCustomRenderer(Context context) : base(context)
        {
            _player = new SimpleExoPlayer.Builder(Context).Build();

            _player.PlayWhenReady = true;
            _player.RepeatMode = (int)RepeatMode.Restart;

            _playerView = new PlayerView(Context)
            {
                UseController = false,
                Player = _player,
                ControllerAutoShow = false
            };
        }

        public AndroidExoPlayerCustomRenderer(System.IntPtr ptr, Android.Runtime.JniHandleOwnership jni) 
        {

        }

        public void OnDownstreamFormatChanged(int windowIndex, MediaSourceMediaPeriodId mediaPeriodId, MediaSourceEventListenerMediaLoadData mediaLoadData)
        {

        }

        public void OnLoadCanceled(int windowIndex, MediaSourceMediaPeriodId mediaPeriodId, MediaSourceEventListenerLoadEventInfo loadEventInfo, MediaSourceEventListenerMediaLoadData mediaLoadData)
        {

        }

        public void OnLoadCompleted(int windowIndex, MediaSourceMediaPeriodId mediaPeriodId, MediaSourceEventListenerLoadEventInfo loadEventInfo, MediaSourceEventListenerMediaLoadData mediaLoadData)
        {

        }

        public void OnLoadError(int windowIndex, MediaSourceMediaPeriodId mediaPeriodId, MediaSourceEventListenerLoadEventInfo loadEventInfo, MediaSourceEventListenerMediaLoadData mediaLoadData, IOException error, bool wasCanceled)
        {

        }

        public void OnLoadStarted(int windowIndex, MediaSourceMediaPeriodId mediaPeriodId, MediaSourceEventListenerLoadEventInfo loadEventInfo, MediaSourceEventListenerMediaLoadData mediaLoadData)
        {

        }

        public void OnMediaPeriodCreated(int windowIndex, MediaSourceMediaPeriodId mediaPeriodId)
        {

        }

        public void OnMediaPeriodReleased(int windowIndex, MediaSourceMediaPeriodId mediaPeriodId)
        {

        }

        public void OnReadingStarted(int windowIndex, MediaSourceMediaPeriodId mediaPeriodId)
        {

        }

        public void OnUpstreamDiscarded(int windowIndex, MediaSourceMediaPeriodId mediaPeriodId, MediaSourceEventListenerMediaLoadData mediaLoadData)
        {

        }

        protected override void Dispose(bool disposing)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<AndroidExoPlayerView> e)
        {
            base.OnElementChanged(e);

            if (Control is null)
                SetNativeControl(_playerView);

            if (e.NewElement.SourceUrl != null)
                Play(e.NewElement.SourceUrl);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName is nameof(AndroidExoPlayerView.SourceUrl) && Element.SourceUrl != null)
                Play(Element.SourceUrl);
        }

        void Play(string url)
        {
            var httpDataSourceFactory = new DefaultHttpDataSourceFactory("1");
            var ssChunkFactory = new DefaultSsChunkSource.Factory(httpDataSourceFactory);

            var ssMediaSource = new SsMediaSource(Uri.Parse(url), httpDataSourceFactory, ssChunkFactory, new Handler(), this);

            _player.Prepare(ssMediaSource);
        }
    }
}