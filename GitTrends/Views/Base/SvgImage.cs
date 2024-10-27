using System.ComponentModel;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;

namespace GitTrends;

public class SvgImage : Image
{
	public static readonly BindableProperty GetSvgColorProperty =
		BindableProperty.Create(nameof(GetSvgColor), typeof(Func<Color>), typeof(SvgImage), () => Colors.White);

	const int _defaultHeight = 18;
	const int _defaultWidth = 18;

	readonly IDeviceInfo _deviceInfo;
	readonly IconTintColorBehavior _iconTintColorBehavior;

	public SvgImage(in IDeviceInfo deviceInfo, in string svgFileName, in Func<Color> svgColor, double widthRequest = _defaultWidth, double heightRequest = _defaultHeight)
		: this(deviceInfo, svgColor, widthRequest, heightRequest)
	{
		if (!Path.GetExtension(svgFileName).EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException($"{nameof(svgFileName)} must end with `.svg`");

		Source = svgFileName;
	}

	public SvgImage(IDeviceInfo deviceInfo, Func<Color> color, double widthRequest = _defaultWidth, double heightRequest = _defaultHeight)
		: this(deviceInfo, widthRequest, heightRequest)
	{
		GetSvgColor = color;
	}

	public SvgImage(IDeviceInfo deviceInfo, double widthRequest = _defaultWidth, double heightRequest = _defaultHeight)
	{
		_deviceInfo = deviceInfo;

		PropertyChanged += HandlePropertyChanged;
		ThemeService.PreferenceChanged += HandlePreferenceChanged;

		this.Fill();

		WidthRequest = widthRequest;
		HeightRequest = heightRequest;

		Behaviors.Add(new IconTintColorBehavior()
			.Bind(IconTintColorBehavior.TintColorProperty,
				getter: static image => image.GetSvgColor,
				convert: static getSvgColor => getSvgColor?.Invoke() ?? Colors.White,
				source: this)
			.Assign(out _iconTintColorBehavior));
	}

	public Func<Color> GetSvgColor
	{
		get => (Func<Color>)GetValue(GetSvgColorProperty);
		set => SetValue(GetSvgColorProperty, value);
	}

	void HandlePreferenceChanged(object? sender, PreferredTheme e)
	{
		_iconTintColorBehavior.TintColor = GetSvgColor();
	}

	void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == SourceProperty.PropertyName)
		{
			if (_deviceInfo.Platform == DevicePlatform.iOS
				&& Source is IFileImageSource fileImageSource
				&& Path.GetExtension(fileImageSource.File).Trim().Equals(".svg", StringComparison.OrdinalIgnoreCase))
			{
				Source = Path.ChangeExtension(fileImageSource.File, ".png");
			}
		}
	}
}