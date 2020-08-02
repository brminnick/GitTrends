using System;
using Xamarin.Forms;
using System.Windows.Input;

//Inspired by https://github.com/jamesmontemagno/FloatingActionButton-for-Xamarin.Android
namespace GitTrends
{
    public class FloatingActionButtonView : View
    {
        public static readonly BindableProperty ImageNameProperty = BindableProperty.Create(nameof(ImageName), typeof(string), typeof(FloatingActionButtonView), string.Empty);
        public static readonly BindableProperty ColorNormalProperty = BindableProperty.Create(nameof(ColorNormal), typeof(Color), typeof(FloatingActionButtonView), Color.White);
        public static readonly BindableProperty RippleColorProperty = BindableProperty.Create(nameof(RippleColor), typeof(Color), typeof(FloatingActionButtonView), Color.White);
        public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(FloatingActionButtonSize), typeof(FloatingActionButtonView), FloatingActionButtonSize.Normal);
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingActionButtonView), null, propertyChanged: (bo, o, n) => ((FloatingActionButtonView)bo).OnCommandChanged());
        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(FloatingActionButtonView), null, propertyChanged: (bindable, oldvalue, newvalue) => ((FloatingActionButtonView)bindable).CommandCanExecuteChanged(bindable, EventArgs.Empty));

        public FloatingActionButtonView()
        {
            if (Device.RuntimePlatform != Device.Android)
                throw new NotSupportedException($"{nameof(FloatingActionButtonView)} is only supported on Android");
        }

        public string ImageName
        {
            get => (string)GetValue(ImageNameProperty);
            set => SetValue(ImageNameProperty, value);
        }

        public Color ColorNormal
        {
            get => (Color)GetValue(ColorNormalProperty);
            set => SetValue(ColorNormalProperty, value);
        }

        public Color RippleColor
        {
            get => (Color)GetValue(RippleColorProperty);
            set => SetValue(RippleColorProperty, value);
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

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public delegate void ShowHideDelegate();

        public ShowHideDelegate? Show { get; set; }
        public ShowHideDelegate? Hide { get; set; }

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

    public enum FloatingActionButtonSize { Normal, Mini }
}