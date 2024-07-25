using AsyncAwaitBestPractices;
using GitTrends.Mobile.Common;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Behaviors;

namespace GitTrends;

public class SvgImage : Image
{
	public static readonly BindableProperty SvgColorProperty =
		BindableProperty.Create(nameof(SvgColor), typeof(Color), typeof(SvgImage), Colors.White, propertyChanged: HandleGetTextColorPropertyChanged);

	public SvgImage(in string svgFileName, in Color svgColor, double widthRequest = 24, double heightRequest = 24)
		: this(svgColor, widthRequest, heightRequest)
	{
		if (!Path.GetExtension(svgFileName).EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException($"{nameof(svgFileName)} must end with `.svg`");

		Source = svgFileName;
	}

	public SvgImage(Color color, double widthRequest = 24, double heightRequest = 24)
		: this(widthRequest, heightRequest)
	{
		SvgColor = color;
	}

	public SvgImage(double widthRequest = 24, double heightRequest = 24)
	{
		ThemeService.PreferenceChanged += HandlePreferenceChanged;

		this.Fill();

		WidthRequest = widthRequest;
		HeightRequest = heightRequest;

		Behaviors.Add(new IconTintColorBehavior()
			.Bind(IconTintColorBehavior.TintColorProperty,
				getter: static image => image.SvgColor,
				source: this));
	}

	public Color SvgColor
	{
		get => (Color)GetValue(SvgColorProperty);
		set
		{
			if (Equals(value, SvgColor))
			{
				SetValue(SvgColorProperty, value);
				OnPropertyChanged();
			}
		}
	}

	static void HandleGetTextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var svgImage = (SvgImage)bindable;
		svgImage.SvgColor = (Color)newValue;
	}

	void HandlePreferenceChanged(object? sender, PreferredTheme e)
	{
		throw new NotImplementedException();
	}
}