using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace GitTrends;

class ExtendedSwipeView : BaseExtendedSwipeView<object?>
{
}

class ExtendedSwipeView<TBindingContext> : BaseExtendedSwipeView<TBindingContext>
{
}

abstract class BaseExtendedSwipeView<TBindingContext> : SwipeView
{
	readonly AsyncAwaitBestPractices.WeakEventManager _tappedWeakEventManager = new();

	public BaseExtendedSwipeView()
	{
		CloseRequested += OnCloseRequested;
		SwipeEnded += OnSwipeEnded;

		var tappedGestureRecognizer = new TapGestureRecognizer();
		tappedGestureRecognizer.Tapped += HandleTapped;

		GestureRecognizers.Add(tappedGestureRecognizer);
	}

	public static readonly BindableProperty TappedCommandProperty = BindableProperty.Create(nameof(TappedCommand), typeof(ICommand), typeof(BaseExtendedSwipeView<TBindingContext>));
	public static readonly BindableProperty TappedCommandParameterProperty = BindableProperty.Create(nameof(TappedCommandParameter), typeof(object), typeof(BaseExtendedSwipeView<TBindingContext>));

	public event EventHandler Tapped
	{
		add => _tappedWeakEventManager.AddEventHandler(value);
		remove => _tappedWeakEventManager.RemoveEventHandler(value);
	}

	public bool IsSwiped { get; private set; }

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

	void OnCloseRequested(object sender, EventArgs e) => IsSwiped = false;

	void OnSwipeEnded(object sender, SwipeEndedEventArgs e)
	{
		if (GetSwipeMode(e.SwipeDirection) is SwipeMode.Reveal)
			IsSwiped = true;
	}

	void HandleTapped(object sender, EventArgs e)
	{
		if (!IsSwiped)
		{
			OnTapped();
			TappedCommand?.Execute(TappedCommandParameter);
		}
		else
		{
			IsSwiped = false;
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

	void OnTapped() =>
		_tappedWeakEventManager.RaiseEvent(this, EventArgs.Empty, nameof(Tapped));
}