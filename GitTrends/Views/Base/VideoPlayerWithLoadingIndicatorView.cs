using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace GitTrends
{
	public class VideoPlayerWithLoadingIndicatorView : Grid
	{
		public static readonly BindableProperty ResourcePathProperty = BindableProperty.Create(nameof(ResourcePath), typeof(string), typeof(VideoPlayerWithLoadingIndicatorView), null);
		public static readonly BindableProperty ActivityIndicatorColorProperty = BindableProperty.Create(nameof(ActivityIndicatorColor), typeof(Color), typeof(VideoPlayerWithLoadingIndicatorView), null);
		public static readonly BindableProperty IsActivityIndicatorVisibleProperty = BindableProperty.Create(nameof(IsActivityIndicatorVisible), typeof(bool), typeof(VideoPlayerWithLoadingIndicatorView), true);
		public static readonly BindableProperty IsActivityIndicatorRunningProperty = BindableProperty.Create(nameof(IsActivityIndicatorRunning), typeof(bool), typeof(VideoPlayerWithLoadingIndicatorView), true);

		public VideoPlayerWithLoadingIndicatorView(string? path)
		{
			ResourcePath = path;

			Padding = 0;

			RowDefinitions = Rows.Define((Row.Video, Star));

			BackgroundColor = Colors.Transparent;

			Children.Add(new ActivityIndicator()
				.Row(Row.Video)
				.Margin(10)
				.Bind(ActivityIndicator.ColorProperty,
					getter: static vm => vm.ActivityIndicatorColor,
					source: this)
				.Bind(ActivityIndicator.IsVisibleProperty,
					getter: static vm => vm.IsActivityIndicatorVisible,
					source: this)
				.Bind(ActivityIndicator.IsRunningProperty,
					getter: static vm => vm.IsActivityIndicatorRunning,
					source: this));

			Children.Add(new MediaElement
				{
					BackgroundColor = Colors.Transparent,
					ShouldAutoPlay = true,
					ShouldShowPlaybackControls = false,
					ShouldLoopPlayback = true,
					Volume = 0.0
				}.Fill()
				.Row(Row.Video)
				.Bind(MediaElement.SourceProperty,
					getter: static vm => vm.ResourcePath,
					source: this,
					convert: static path => MediaSource.FromResource(path)));
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

		public string? ResourcePath
		{
			get => (string?)GetValue(ResourcePathProperty);
			set => SetValue(ResourcePathProperty, value);
		}
	}
}