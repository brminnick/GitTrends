using FFImageLoading.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;

namespace GitTrends
{
    public class CircleImage : CirclePancakeView, IBorderElement
    {
        public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(CircleImage), Aspect.AspectFit);
        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(CircleImage), null);
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(CircleImage), default(Color),propertyChanged: HandleBorderColorChanged);
        public static readonly BindableProperty ErrorPlaceholderProperty = BindableProperty.Create(nameof(ErrorPlaceholder), typeof(ImageSource), typeof(CircleImage), null);
        public static readonly BindableProperty LoadingPlaceholderProperty = BindableProperty.Create(nameof(LoadingPlaceholder), typeof(ImageSource), typeof(CircleImage), null);

        public CircleImage(in ImageSource imageSource, in ImageSource errorPlaceholder, in ImageSource loadingPlaceholder) : this()
        {
            ImageSource = imageSource;
            ErrorPlaceholder = errorPlaceholder;
            LoadingPlaceholder = loadingPlaceholder;
        }

        public CircleImage()
        {
            Content = new CachedImage()
                        .Bind(CachedImage.SourceProperty, nameof(ImageSource), source: this)
                        .Bind(CachedImage.ErrorPlaceholderProperty, nameof(ErrorPlaceholder), source: this)
                        .Bind(CachedImage.LoadingPlaceholderProperty, nameof(LoadingPlaceholder), source: this);
        }

        public Color BorderColor
        {
            get => (Color)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public Aspect Aspect
        {
            get => (Aspect)GetValue(AspectProperty);
            set => SetValue(AspectProperty, value);
        }

        public ImageSource? ImageSource
        {
            get => (ImageSource?)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public ImageSource? ErrorPlaceholder
        {
            get => (ImageSource?)GetValue(ErrorPlaceholderProperty);
            set => SetValue(ErrorPlaceholderProperty, value);
        }

        public ImageSource? LoadingPlaceholder
        {
            get => (ImageSource?)GetValue(LoadingPlaceholderProperty);
            set => SetValue(LoadingPlaceholderProperty, value);
        }

        int IBorderElement.CornerRadiusDefaultValue => default;
        Color IBorderElement.BorderColorDefaultValue => default;
        double IBorderElement.BorderWidthDefaultValue => default;

        int IBorderElement.CornerRadius => (int)CornerRadius.TopLeft;
        Color IBorderElement.BorderColor => Border.Color;
        double IBorderElement.BorderWidth => Border.Thickness;

        static void HandleBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var pancakeView = (PancakeView)bindable;
            pancakeView.Border.Color = (Color)newValue;
        }

        bool IBorderElement.IsCornerRadiusSet() => true;
        bool IBorderElement.IsBackgroundColorSet() => true;
        bool IBorderElement.IsBackgroundSet() => true;
        bool IBorderElement.IsBorderColorSet() =>true;
        bool IBorderElement.IsBorderWidthSet() => true;
        void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue) => Border.Color = newValue;
    }
}
