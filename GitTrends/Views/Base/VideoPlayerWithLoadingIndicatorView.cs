using System;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace GitTrends
{
	public class VideoPlayerWithLoadingIndicatorView : Grid
	{
		public static readonly BindableProperty UrlProperty = BindableProperty.Create(nameof(Url), typeof(string), typeof(VideoPlayerWithLoadingIndicatorView), null);
		public static readonly BindableProperty ActivityIndicatorColorProperty = BindableProperty.Create(nameof(ActivityIndicatorColor), typeof(Color), typeof(VideoPlayerWithLoadingIndicatorView), null);
		public static readonly BindableProperty IsActivityIndicatorVisibleProperty = BindableProperty.Create(nameof(IsActivityIndicatorVisible), typeof(bool), typeof(VideoPlayerWithLoadingIndicatorView), true);
		public static readonly BindableProperty IsActivityIndicatorRunningProperty = BindableProperty.Create(nameof(IsActivityIndicatorRunning), typeof(bool), typeof(VideoPlayerWithLoadingIndicatorView), true);

		readonly VideoPlayerView _videoPlayerView;
		readonly ActivityIndicator _activityIndicator = new();

		public VideoPlayerWithLoadingIndicatorView(string? uri)
		{
			_videoPlayerView = new VideoPlayerView(uri);
			Url = uri;

			Padding = 0;

			RowDefinitions = Rows.Define((Row.Video, Star));

			Children.Add(_activityIndicator
							.Row(Row.Video)
							.Bind(ActivityIndicator.ColorProperty, nameof(ActivityIndicatorColor), source: this)
							.Bind(ActivityIndicator.IsVisibleProperty, nameof(IsActivityIndicatorVisible), source: this)
							.Bind(ActivityIndicator.IsRunningProperty, nameof(IsActivityIndicatorRunning), source: this));

			Children.Add(_videoPlayerView
							.Row(Row.Video)
							.Bind(VideoPlayerView.UrlProperty, nameof(Url), source: this));
		}

		enum Row { Video }

		public Color? ActivityIndicatorColor
		{
			get => (Color?)GetValue(ActivityIndicatorColorProperty);
			set => SetValue(ActivityIndicatorColorProperty, value);
		}

		public bool IsActivityIndicatorVisible
		{
			get => (bool)GetValue(IsActivityIndicatorVisibleProperty);
			set => SetValue(IsActivityIndicatorVisibleProperty, value);
		}

		public bool IsActivityIndicatorRunning
		{
			get => (bool)GetValue(IsActivityIndicatorRunningProperty);
			set => SetValue(IsActivityIndicatorRunningProperty, value);
		}

		public string? Url
		{
			get => (string?)GetValue(UrlProperty);
			set => SetValue(UrlProperty, value);
		}
	}
}

