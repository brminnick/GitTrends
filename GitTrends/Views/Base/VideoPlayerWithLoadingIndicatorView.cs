using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends
{
	public class VideoPlayerWithLoadingIndicatorView : Grid
	{
		public static readonly BindableProperty UrlProperty = BindableProperty.Create(nameof(Url), typeof(string), typeof(VideoPlayerWithLoadingIndicatorView), null);
		public static readonly BindableProperty ActivityIndicatorColorProperty = BindableProperty.Create(nameof(ActivityIndicatorColor), typeof(Color), typeof(VideoPlayerWithLoadingIndicatorView), null);
		public static readonly BindableProperty IsActivityIndicatorVisibleProperty = BindableProperty.Create(nameof(IsActivityIndicatorVisible), typeof(bool), typeof(VideoPlayerWithLoadingIndicatorView), true);
		public static readonly BindableProperty IsActivityIndicatorRunningProperty = BindableProperty.Create(nameof(IsActivityIndicatorRunning), typeof(bool), typeof(VideoPlayerWithLoadingIndicatorView), true);

		public VideoPlayerWithLoadingIndicatorView(string? uri)
		{
			Url = uri;

			Padding = 0;

			RowDefinitions = Rows.Define((Row.Video, Star));

			Children.Add(new ActivityIndicator()
							.Row(Row.Video)
							.Margin(10)
							.Bind(ActivityIndicator.ColorProperty, nameof(ActivityIndicatorColor), source: this)
							.Bind(ActivityIndicator.IsVisibleProperty, nameof(IsActivityIndicatorVisible), source: this)
							.Bind(ActivityIndicator.IsRunningProperty, nameof(IsActivityIndicatorRunning), source: this));

			Children.Add(new MediaElement()
							.Row(Row.Video)
							.Bind(MediaElement.SourceProperty, nameof(Url), source: this));
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