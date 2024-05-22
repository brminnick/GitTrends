using AsyncAwaitBestPractices;
using GitTrends.Mobile.Common;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Behaviors;

namespace GitTrends;

public partial class SvgImage : Image
{
	public static readonly BindableProperty GetColorProperty =
		BindableProperty.Create(nameof(GetColor), typeof(Func<Color>), typeof(SvgImage), () => Colors.Transparent, propertyChanged: HandleGetTextColorPropertyChanged);

	readonly IconTintColorBehavior _iconTintColorBehavior;

	public SvgImage(in string svgFileName, in Func<Color> getColor, double widthRequest = 24, double heightRequest = 24)
		: this(getColor, widthRequest, heightRequest)
	{
		Source = SvgService.GetValidatedFullPath(svgFileName);
	}

	public SvgImage(in Func<Color> getColor, double widthRequest = 24, double heightRequest = 24)
		: this(widthRequest, heightRequest)
	{
		GetColor = getColor;
	}

	public SvgImage(double widthRequest = 24, double heightRequest = 24)
	{
		ThemeService.PreferenceChanged += HandlePreferenceChanged;

		this.Fill();

		WidthRequest = widthRequest;
		HeightRequest = heightRequest;

		Behaviors.Add(new IconTintColorBehavior()
						.Bind(IconTintColorBehavior.TintColorProperty, 
							getter: static (SvgImage image) => image.GetColor(),
							source: this)
						.Assign(out _iconTintColorBehavior));
	}

	public Func<Color> GetColor
	{
		get => (Func<Color>)GetValue(GetColorProperty);
		set
		{
			SetSvgColorAsync().SafeFireAndForget();

			if (value() != GetColor())
			{
				SetValue(GetColorProperty, value);
				OnPropertyChanged(nameof(GetColor));
			}
		}
	}

	static void HandleGetTextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var svgImage = (SvgImage)bindable;
		svgImage.GetColor = (Func<Color>)newValue;
	}

	void HandlePreferenceChanged(object? sender, PreferredTheme e) => SetSvgColor();

	Task SetSvgColorAsync() => Dispatcher.DispatchAsync(SetSvgColor);

	void SetSvgColor() => _iconTintColorBehavior.TintColor = GetColor();
}