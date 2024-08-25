using System.ComponentModel;
using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;

namespace GitTrends;

public class CircleImage : CircleBorder
{
	public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(CircleImage), Aspect.AspectFit);
	public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(CircleImage), null);
	public static readonly BindableProperty GetBorderColorProperty = BindableProperty.Create(nameof(GetBorderColor), typeof(Func<Color>), typeof(CircleImage), () => default(Color));
	public static readonly BindableProperty ErrorPlaceholderProperty = BindableProperty.Create(nameof(ErrorPlaceholder), typeof(ImageSource), typeof(CircleImage), null);
	public static readonly BindableProperty LoadingPlaceholderProperty = BindableProperty.Create(nameof(LoadingPlaceholder), typeof(ImageSource), typeof(CircleImage), null);

	public CircleImage(in ImageSource imageSource, in ImageSource errorPlaceholder, in ImageSource loadingPlaceholder) : this()
	{
		ImageSource = imageSource;
		ErrorPlaceholder = errorPlaceholder;
		LoadingPlaceholder = loadingPlaceholder;
	}

	public CircleImage()
	{
		this.Bind(StrokeProperty, 
			nameof(GetBorderColor), 
			source: this,
			convert: static (Func<Color>? getColorFunc) => getColorFunc?.Invoke() ?? default);

		Content = new Image()
			.Bind(Image.AspectProperty, nameof(Aspect), source: this)
			.Bind(Image.SourceProperty, nameof(ImageSource), source: this)
			.Invoke(image => image.PropertyChanged += HandleCircleImagePropertyChanged);

		ThemeService.PreferenceChanged += HandleThemePreferenceChanged;
	}

	public Func<Color> GetBorderColor
	{
		get => (Func<Color>)GetValue(GetBorderColorProperty);
		set => SetValue(GetBorderColorProperty, value);
	}

	public Aspect Aspect
	{
		get => (Aspect)GetValue(AspectProperty);
		set => SetValue(AspectProperty, value);
	}

	public ImageSource? ImageSource
	{
		get => (ImageSource?)GetValue(ImageSourceProperty);
		set => SetValue(ImageSourceProperty, value);
	}

	public ImageSource? ErrorPlaceholder
	{
		get => (ImageSource?)GetValue(ErrorPlaceholderProperty);
		set => SetValue(ErrorPlaceholderProperty, value);
	}

	public ImageSource? LoadingPlaceholder
	{
		get => (ImageSource?)GetValue(LoadingPlaceholderProperty);
		set => SetValue(LoadingPlaceholderProperty, value);
	}

	async void HandleCircleImagePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);

		var image = (Image)sender;

		if (e.PropertyName != Image.IsLoadingProperty.PropertyName || image.Source == ImageSource) 
			return;

		if (image.IsLoading)
		{
			image.Source = LoadingPlaceholder;
		}
		else
		{
			if (image.Handler?.MauiContext is null)
			{
				throw new InvalidOperationException("Handler cannot be null");
			}

			try
			{
				await image.Source.GetPlatformImageAsync(image.Handler.MauiContext);
				image.Source = ImageSource;
			}
			catch
			{
				image.Source = ErrorPlaceholder;
			}
		}
	}
	
	void HandleThemePreferenceChanged(object? sender, PreferredTheme e)
	{
		Stroke = GetBorderColor();
	}
}