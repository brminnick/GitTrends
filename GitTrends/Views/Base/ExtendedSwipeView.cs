using System;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using Xamarin.Forms;

namespace GitTrends
{
    public class ExtendedSwipeView : SwipeView
    {
        public ExtendedSwipeView()
        {
            CloseRequested += OnCloseRequested;
            SwipeEnded += OnSwipeEnded;

            var tappedGestureRecognizer = new TapGestureRecognizer();
            tappedGestureRecognizer.Tapped += HandleTapped;

            GestureRecognizers.Add(tappedGestureRecognizer);
        }

        public static readonly BindableProperty TappedCommandProperty = BindableProperty.Create(nameof(TappedCommand), typeof(ICommand), typeof(ExtendedSwipeView));
        public static readonly BindableProperty TappedCommandParameterProperty = BindableProperty.Create(nameof(TappedCommandParameter), typeof(object), typeof(ExtendedSwipeView));

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
                TappedCommand?.Execute(TappedCommandParameter);
            else
                IsSwiped = false;
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
}