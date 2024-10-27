using System.Windows.Input;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Layouts;

namespace GitTrends;

class FloatingActionButtonView : AbsoluteLayout
{
	const int _fabDiameterMini = 44;
	const int _fabDiameterNormal = 56;

	const int _shadowDiameterMini = _fabDiameterMini + 1;
	const int _shadowDiameterNormal = _fabDiameterNormal + 2;

	public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(FloatingActionButtonSize), typeof(FloatingActionButtonView), FloatingActionButtonSize.Normal, propertyChanged: OnSizeChanged);
	public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingActionButtonView), null, propertyChanged: (bindable, _, _) => ((FloatingActionButtonView)bindable).OnCommandChanged());
	public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(FloatingActionButtonView), null, propertyChanged: (bindable, _, _) => ((FloatingActionButtonView)bindable).CommandCanExecuteChanged(bindable, EventArgs.Empty));
	public static readonly BindableProperty FloatingActionButtonBackgroundColorProperty = BindableProperty.Create(nameof(FloatingActionButtonBackgroundColor), typeof(Color), typeof(FloatingActionButtonView), Colors.Transparent, propertyChanged: OnFloatingActionBackgroundColorChanged);

	public FloatingActionButtonView()
	{
		WidthRequest = _shadowDiameterNormal + 10;
		HeightRequest = _shadowDiameterNormal + 10;

		Shadow = new CircleBorder
		{
			WidthRequest = _shadowDiameterNormal,
			HeightRequest = _shadowDiameterNormal,
			BackgroundColor = Color.FromArgb(DarkTheme.PageBackgroundColorHex),
			Opacity = 0.2
		}.Center();

		FloatingActionButton = new CircleBorder
		{
			WidthRequest = _fabDiameterNormal,
			HeightRequest = _fabDiameterNormal
		}.Center();

		FloatingActionButton.GestureRecognizers.Add(new TapGestureRecognizer().Invoke(tapGesture => tapGesture.Tapped += HandleTapped));

		Children.Add(Shadow
						.LayoutBounds(new Rect(.51, .55, -1, -1))
						.LayoutFlags(AbsoluteLayoutFlags.PositionProportional));

		Children.Add(FloatingActionButton.
						LayoutBounds(new Rect(.5, .5, -1, -1))
						.LayoutFlags(AbsoluteLayoutFlags.PositionProportional));
	}

	public new CircleBorder Shadow { get; }
	public CircleBorder FloatingActionButton { get; }

	public View? Content
	{
		get => FloatingActionButton.Content;
		set => FloatingActionButton.Content = value;
	}

	public Color FloatingActionButtonBackgroundColor
	{
		get => (Color)GetValue(FloatingActionButtonBackgroundColorProperty);
		set => SetValue(FloatingActionButtonBackgroundColorProperty, value);
	}

	public FloatingActionButtonSize Size
	{
		get => (FloatingActionButtonSize)GetValue(SizeProperty);
		set => SetValue(SizeProperty, value);
	}

	public ICommand? Command
	{
		get => (ICommand?)GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}

	public object? CommandParameter
	{
		get => (object?)GetValue(CommandParameterProperty);
		set => SetValue(CommandParameterProperty, value);
	}

	static void OnFloatingActionBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var floatingActionButtonView = (FloatingActionButtonView)bindable;
		var color = (Color)newValue;

		floatingActionButtonView.FloatingActionButton.BackgroundColor = color;
	}

	static void OnSizeChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var floatingActionButtonView = (FloatingActionButtonView)bindable;
		var size = (FloatingActionButtonSize)newValue;

		floatingActionButtonView.Shadow.WidthRequest = floatingActionButtonView.Shadow.HeightRequest = size switch
		{
			FloatingActionButtonSize.Mini => _shadowDiameterMini,
			FloatingActionButtonSize.Normal => _shadowDiameterNormal,
			_ => throw new NotImplementedException()
		};

		floatingActionButtonView.FloatingActionButton.WidthRequest = floatingActionButtonView.FloatingActionButton.HeightRequest = size switch
		{
			FloatingActionButtonSize.Mini => _fabDiameterMini,
			FloatingActionButtonSize.Normal => _fabDiameterNormal,
			_ => throw new NotImplementedException()
		};
	}

	void HandleTapped(object? sender, EventArgs e)
	{
		if (Command?.CanExecute(CommandParameter) is true)
			Command.Execute(CommandParameter);
	}

	void OnCommandChanged()
	{
		if (Command != null)
		{
			Command.CanExecuteChanged += CommandCanExecuteChanged;
			CommandCanExecuteChanged(this, EventArgs.Empty);
		}
		else
		{
			IsEnabled = true;
		}
	}

	void CommandCanExecuteChanged(object? sender, EventArgs eventArgs) => IsEnabled = Command?.CanExecute(CommandParameter) ?? false;
}

enum FloatingActionButtonSize { Normal, Mini }