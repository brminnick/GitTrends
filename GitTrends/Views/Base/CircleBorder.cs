using CommunityToolkit.Maui.Markup;
using GitTrends.Mobile.Common;
using Microsoft.Maui.Controls.Shapes;

namespace GitTrends;

public class CircleBorder : Border
{
	public static readonly BindableProperty GetBorderColorProperty = BindableProperty.Create(nameof(GetBorderColor), typeof(Func<Color>), typeof(CircleImage), () => default(Color));

	public CircleBorder()
	{
		ThemeService.PreferenceChanged += HandleThemePreferenceChanged;

		HeightRequest = WidthRequest = Math.Min(HeightRequest, WidthRequest);

		this.Bind(StrokeProperty,
			nameof(GetBorderColor),
			source: this,
			convert: static (Func<Color>? getColorFunc) => getColorFunc?.Invoke() ?? null);

		StrokeShape = new RoundRectangle()
			.Bind(RoundRectangle.CornerRadiusProperty,
				getter: static circleBorder => circleBorder.Width,
				convert: ConvertWidthToCornerRadius,
				source: this);

		static CornerRadius ConvertWidthToCornerRadius(double width) => width is -1 ? -1 : width / 2;
	}

	public Func<Color> GetBorderColor
	{
		get => (Func<Color>)GetValue(GetBorderColorProperty);
		set => SetValue(GetBorderColorProperty, value);
	}

	void HandleThemePreferenceChanged(object? sender, PreferredTheme e)
	{
		Stroke = GetBorderColor();
	}
}