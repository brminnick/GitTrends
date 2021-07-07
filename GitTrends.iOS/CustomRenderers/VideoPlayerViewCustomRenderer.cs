using System;
using System.ComponentModel;
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
        readonly static AVQueuePlayer _queuePlayer = new() { Volume = 0 };
        readonly static AVPlayerViewController _avPlayerViewController = new();

        static AVPlayerLooper? _avPlayerLooper;
        static AVPlayerItem? _onboardingChartItem;

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

        static AVPlayerLooper CreateAVPlayerLooper(AVPlayerItem onboardingChartItem) => new(_queuePlayer, onboardingChartItem, CMTimeRange.InvalidRange);

        void Play(string videoUrl, UIView avPlayerViewControllerView)
        {
            _onboardingChartItem = CreatePlayerItem(videoUrl);
            _avPlayerLooper = CreateAVPlayerLooper(_onboardingChartItem);
            avPlayerViewControllerView.BackgroundColor = Color.White.ToUIColor();

            SetNativeControl(avPlayerViewControllerView);

            _avPlayerViewController.ShowsPlaybackControls = false;
            _avPlayerViewController.VideoGravity = AVLayerVideoGravity.ResizeAspect;

            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSession.CategoryPlayback);
            audioSession.SetMode(AVAudioSession.ModeMoviePlayback, out _);
            audioSession.SetActive(false);

            _avPlayerViewController.Player = _queuePlayer;
            _avPlayerViewController.Player.Play();
        }

        AVPlayerItem CreatePlayerItem(string url)
        {
            var asset = AVUrlAsset.Create(NSUrl.FromString(url));

            return new AVPlayerItem(asset)
            {
                PreferredForwardBufferDuration = 1,
            };
        }
    }
}