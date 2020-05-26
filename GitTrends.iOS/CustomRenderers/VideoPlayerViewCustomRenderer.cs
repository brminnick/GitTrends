using System;
using Autofac;
using AVFoundation;
using AVKit;
using CoreGraphics;
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
        readonly static AVQueuePlayer _queuePlayer = new AVQueuePlayer
        {
            Volume = 0,
        };

        readonly static AVPlayerItem _onboardingChartItem = CreateOnboardingChartItem();
        readonly static AVPlayerViewController _avPlayerViewController = new AVPlayerViewController();
        readonly static AVPlayerLooper _avPlayerLooper = CreateAVPlayerLooper();

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayerView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null && _avPlayerViewController.View != null)
            {
                _avPlayerViewController.View.BackgroundColor = Color.White.ToUIColor();

                SetNativeControl(_avPlayerViewController.View);

                _avPlayerViewController.ShowsPlaybackControls = false;
                _avPlayerViewController.VideoGravity = AVLayerVideoGravity.ResizeAspect;

                var audioSession = AVAudioSession.SharedInstance();
                audioSession.SetCategory(AVAudioSession.CategoryPlayback);
                audioSession.SetMode(AVAudioSession.ModeMoviePlayback, out _);
                audioSession.SetActive(false);

                _avPlayerViewController.Player = _queuePlayer;
                _avPlayerViewController.Player.Play();
            }
        }

        static AVPlayerLooper CreateAVPlayerLooper() => new AVPlayerLooper(_queuePlayer, _onboardingChartItem, CMTimeRange.InvalidRange);

        static AVPlayerItem CreateOnboardingChartItem()
        {
            using var scope = ContainerService.Container.BeginLifetimeScope();
            var mediaElementService = scope.Resolve<MediaElementService>();

            if (mediaElementService.OnboardingChart?.HlsUrl is null)
                throw new NullReferenceException();

            var asset = AVUrlAsset.Create(NSUrl.FromString(mediaElementService.OnboardingChart.HlsUrl));

            return new AVPlayerItem(asset)
            {
                PreferredForwardBufferDuration = 1,
            };
        }
    }
}