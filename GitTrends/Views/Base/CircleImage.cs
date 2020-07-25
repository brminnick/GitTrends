using FFImageLoading.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;

namespace GitTrends
{
    public class CircleImage : PancakeView
    {
        public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(CircleImage), Aspect.AspectFit);
        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(CircleImage), null);
        public static readonly BindableProperty ErrorPlaceholderProperty = BindableProperty.Create(nameof(ErrorPlaceholder), typeof(ImageSource), typeof(CircleImage), null);
        public static readonly BindableProperty LoadingPlaceholderProperty = BindableProperty.Create(nameof(LoadingPlaceholder), typeof(ImageSource), typeof(CircleImage), null);

        public CircleImage(ImageSource imageSource, ImageSource errorPlaceholder, ImageSource loadingPlaceholder) : this()
        {
            ImageSource = imageSource;
            ErrorPlaceholder = errorPlaceholder;
            LoadingPlaceholder = loadingPlaceholder;
        }

        public CircleImage()
        {
            this.Bind<PancakeView, double, double>(CornerRadiusProperty, nameof(Width), convert: convertWidthToCornerRadius, source: this);

            IsClippedToBounds = true;

            Content = new CachedImage()
                        .Bind(CachedImage.SourceProperty, nameof(ImageSource), source: this)
                        .Bind(CachedImage.ErrorPlaceholderProperty, nameof(ErrorPlaceholder), source: this)
                        .Bind(CachedImage.LoadingPlaceholderProperty, nameof(LoadingPlaceholder), source: this);

            static double convertWidthToCornerRadius(double width) => width is -1 ? -1 : width / 2;
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
    }
}
