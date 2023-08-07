using System.ComponentModel;
using Android.Content;
using Android.Net;
using Android.Views.Animations;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source.Dash;
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
			_player = new SimpleExoPlayer.Builder(context).Build() ?? throw new System.InvalidOperationException();

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
#pragma warning disable IDE0060 // Remove unused parameter
		public VideoPlayerViewCustomRenderer(System.IntPtr ptr, Android.Runtime.JniHandleOwnership jni)
		{
			//Fixes no constructor found exception
		}
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning restore IDE0060 // Remove unused parameter

		protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayerView> e)
		{
			base.OnElementChanged(e);

			if (Control is null)
			{
				SetNativeControl(_playerView);
				SetBackgroundColor(Android.Graphics.Color.Transparent);
			}

			if (Element.Url is not null
				&& Uri.Parse(Element.Url) is Uri uri)
			{
				Play(uri);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VideoPlayerView.UrlProperty.PropertyName
				&& Element.Url is not null
				&& Uri.Parse(Element.Url) is Uri uri)
			{
				Play(uri);
			}
		}

		void Play(in Uri uri)
		{
			var httpDataSourceFactory = new DefaultHttpDataSource.Factory();
			var mediaSource = new DashMediaSource.Factory(httpDataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));

			_player.SetMediaSource(mediaSource);
			_player.Prepare();
		}
	}
}