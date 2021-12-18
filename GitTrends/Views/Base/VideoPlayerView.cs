using Xamarin.Forms;

namespace GitTrends;

//On iOS, use custom renderer for MediaElement until MediaElement.Dispose bug is fixed: https://github.com/xamarin/Xamarin.Forms/issues/9525#issuecomment-619156536
//On Android, use Custom Renderer for ExoPlayer because Xamarin.Forms.MediaElement uses Android.VideoView
public class VideoPlayerView : View
{
	public readonly BindableProperty UrlProperty = BindableProperty.Create(nameof(Url), typeof(string), typeof(VideoPlayerView));

	public VideoPlayerView(string? uri)
	{
		BackgroundColor = Color.Transparent;
		Url = uri;
	}

	public string? Url
	{
		get => (string?)GetValue(UrlProperty);
		set => SetValue(UrlProperty, value);
	}
}