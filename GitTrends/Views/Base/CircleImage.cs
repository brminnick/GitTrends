using FFImageLoading.Forms;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;

namespace GitTrends
{
    public class CircleImage : CirclePancakeView
    {
        public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(CircleImage), Aspect.AspectFit);
        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(CircleImage), null);
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(CircleImage), default(Color));
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
            Border ??= new Border();
            this.Bind(Frame.BorderColorProperty, nameof(BorderColor), source: this);

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
    }
}
