using System.Windows.Input;

namespace GitTrends;

class GitHubButton : SvgTextLabel
{
	public static readonly BindableProperty CommandProperty =
		BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(GitHubButton), null, propertyChanged: OnCommandPropertyChanged);

	public static readonly BindableProperty CommandParameterProperty =
		BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(GitHubButton), null, propertyChanged: OnCommandParameterPropertyChanged);

	public GitHubButton(in IDeviceInfo deviceInfo, in string automationId, in string buttonText)
		: base(deviceInfo, "github.svg", buttonText, automationId, 18, FontFamilyConstants.RobotoRegular, 16)
	{
		BackgroundColor = Color.FromArgb("231F20");

		GestureRecognizers.Add(TapGestureRecognizer);
	}

	public TapGestureRecognizer TapGestureRecognizer { get; } = new();

	public ICommand? Command
	{
		get => (ICommand?)GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}

	public object? CommandParameter
	{
		get => GetValue(CommandParameterProperty);
		set => SetValue(CommandParameterProperty, value);
	}

	static void OnCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var gitHubButton = (GitHubButton)bindable;
		var command = (ICommand)newValue;

		gitHubButton.TapGestureRecognizer.Command = command;
	}

	static void OnCommandParameterPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var gitHubButton = (GitHubButton)bindable;
		var commandParameter = (object?)newValue;

		gitHubButton.TapGestureRecognizer.CommandParameter = commandParameter;
	}
}