using System.Windows.Input;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.Input;

namespace GitTrends;

abstract class ExtendedSwipeView : SwipeView
{
	public static readonly BindableProperty TappedCommandProperty = BindableProperty.Create(nameof(TappedCommand), typeof(ICommand), typeof(ExtendedSwipeView));
	public static readonly BindableProperty TappedCommandParameterProperty = BindableProperty.Create(nameof(TappedCommandParameter), typeof(object), typeof(ExtendedSwipeView));

	protected ExtendedSwipeView()
	{
		SwipeEnded += HandleSwipeEnded;
		SwipeChanging += HandleSwipeChanging;
		CloseRequested += HandleCloseRequested;

		Behaviors.Add(new TouchBehavior
		{
			Command = new RelayCommand(HandleTouch)
		});
	}

	public bool IsSwiped { get; private set; }
	public bool IsSwiping { get; private set; }

	public ICommand? TappedCommand
	{
		get => (ICommand?)GetValue(TappedCommandProperty);
		set => SetValue(TappedCommandProperty, value);
	}

	public object? TappedCommandParameter
	{
		get => GetValue(TappedCommandParameterProperty);
		set => SetValue(TappedCommandParameterProperty, value);
	}

	void HandleSwipeChanging(object? sender, SwipeChangingEventArgs e)
	{
		IsSwiping = IsSwiped = GetSwipeMode(e.SwipeDirection) is SwipeMode.Reveal;
	}

	void HandleCloseRequested(object? sender, CloseRequestedEventArgs e)
	{
		IsSwiped = false;
	}

	void HandleSwipeEnded(object? sender, SwipeEndedEventArgs e)
	{
		IsSwiping = false;
		IsSwiped = e.IsOpen;
	}

	void HandleTouch()
	{
		if (IsSwiping)
			return;

		if (IsSwiped)
		{
			Close();
		}
		else
		{
			if (TappedCommand?.CanExecute(TappedCommandParameter) is true)
				TappedCommand.Execute(TappedCommandParameter);
		}
	}

	SwipeMode GetSwipeMode(SwipeDirection swipeDirection) => swipeDirection switch
	{
		SwipeDirection.Down => TopItems.Mode,
		SwipeDirection.Left => RightItems.Mode,
		SwipeDirection.Up => BottomItems.Mode,
		SwipeDirection.Right => LeftItems.Mode,
		_ => throw new NotSupportedException()
	};
}