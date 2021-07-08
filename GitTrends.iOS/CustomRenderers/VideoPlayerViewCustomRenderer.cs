using System.ComponentModel;
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
        readonly static AVPlayerViewController _avPlayerViewController = new();

        static AVPlayerLooper? _avPlayerLooper;
        static AVPlayerItem? _playerItem;

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayerView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement is not null
                && _avPlayerViewController.View is not null
                && Element.Url is not null)
            {
                Play(Element.Url, _avPlayerViewController.View);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName is nameof(Element.Url)
                && _avPlayerViewController.View is not null
                && Element.Url is not null)
            {
                Play(Element.Url, _avPlayerViewController.View);
            }
        }

        static AVPlayerLooper CreateAVPlayerLooper(AVQueuePlayer queuePlayer, AVPlayerItem onboardingChartItem) => new(queuePlayer, onboardingChartItem, CMTimeRange.InvalidRange);

        static AVPlayerItem CreatePlayerItem(string url)
        {
            var asset = AVUrlAsset.Create(NSUrl.FromString(url));

            return new AVPlayerItem(asset)
            {
                PreferredForwardBufferDuration = 1,
            };
        }

        void Play(string videoUrl, UIView avPlayerViewControllerView)
        {
            var queuePlayer = new AVQueuePlayer() { Volume = 0 };

            _playerItem = CreatePlayerItem(videoUrl);
            _avPlayerLooper = CreateAVPlayerLooper(queuePlayer, _playerItem);
            avPlayerViewControllerView.BackgroundColor = Color.White.ToUIColor();

            SetNativeControl(avPlayerViewControllerView);

            _avPlayerViewController.ShowsPlaybackControls = false;
            _avPlayerViewController.VideoGravity = AVLayerVideoGravity.ResizeAspect;

            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSession.CategoryPlayback);
            audioSession.SetMode(AVAudioSession.ModeMoviePlayback, out _);
            audioSession.SetActive(false);

            _avPlayerViewController.Player = queuePlayer;
            _avPlayerViewController.Player.Play();
        }
    }
}