using System;
using Xamarin.Forms;
using System.Windows.Input;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    class FloatingActionButtonView : CirclePancakeView
    {
        const int _diameterNormal = 56;
        const int _diameterMini = 40;

        public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(FloatingActionButtonSize), typeof(FloatingActionButtonView), FloatingActionButtonSize.Normal, propertyChanged: OnSizeChanged);
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingActionButtonView), null, propertyChanged: (bo, o, n) => ((FloatingActionButtonView)bo).OnCommandChanged());
        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(FloatingActionButtonView), null, propertyChanged: (bindable, oldvalue, newvalue) => ((FloatingActionButtonView)bindable).CommandCanExecuteChanged(bindable, EventArgs.Empty));

        public FloatingActionButtonView()
        {
            HeightRequest = WidthRequest = _diameterNormal;

            GestureRecognizers.Add(new TapGestureRecognizer().Invoke(tapGesture => tapGesture.Tapped += HandleTapped));
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

        static void OnSizeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var floatingActionButton = (FloatingActionButtonView)bindable;
            var size = (FloatingActionButtonSize)newValue;

            floatingActionButton.HeightRequest = floatingActionButton.WidthRequest = size switch
            {
                FloatingActionButtonSize.Normal => _diameterNormal,
                FloatingActionButtonSize.Mini => _diameterMini,
                _ => throw new NotImplementedException()
            };
        }

        void HandleTapped(object sender, EventArgs e)
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

        void CommandCanExecuteChanged(object sender, EventArgs eventArgs) => IsEnabled = Command?.CanExecute(CommandParameter) ?? false;
    }

    enum FloatingActionButtonSize { Normal, Mini }
}