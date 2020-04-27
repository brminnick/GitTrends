using AVFoundation;
using AVKit;
using CoreMedia;
using Foundation;
using GitTrends;
using GitTrends.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(VideoPlayerView), typeof(MediaElementCustomRenderer))]
namespace GitTrends.iOS
{
    public class MediaElementCustomRenderer : ViewRenderer<VideoPlayerView, UIView>
    {
        readonly AVPlayerViewController _avPlayerViewController = new AVPlayerViewController();
        NSObject? _playedToEndObserver;

        public MediaElementCustomRenderer() =>
            _playedToEndObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, PlayedToEnd);

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayerView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                _avPlayerViewController.View.BackgroundColor = Color.White.ToUIColor();

                SetNativeControl(_avPlayerViewController.View);

                _avPlayerViewController.ShowsPlaybackControls = false;
                _avPlayerViewController.VideoGravity = AVLayerVideoGravity.ResizeAspect;

                var asset = AVUrlAsset.Create(NSUrl.FromString(MediaElementService.OnboardingChart?.HlsUrl));

                var item = new AVPlayerItem(asset);

                _avPlayerViewController.Player = new AVPlayer(item)
                {
                    Volume = 0
                };

                var audioSession = AVAudioSession.SharedInstance();
                audioSession.SetCategory(AVAudioSession.CategoryPlayback);
                audioSession.SetMode(AVAudioSession.ModeMoviePlayback, out _);
                audioSession.SetActive(false);

                _avPlayerViewController.Player.Play();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_playedToEndObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_playedToEndObserver);
                _playedToEndObserver = null;
            }

            base.Dispose(disposing);
        }

        void PlayedToEnd(NSNotification notification)
        {
            _avPlayerViewController.Player.Seek(CMTime.Zero);
            _avPlayerViewController.Player.Play();
        }
    }
}